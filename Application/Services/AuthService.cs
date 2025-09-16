using AutoMapper;
using Microsoft.Extensions.Options;
using PrintingTools.Application.DTOs.Auth;
using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.Users;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.Services;
using PrintingTools.Domain.ValueObjects;
using PrintingTools.Infrastructure.Repositories;
using PrintingTools.Settings;

namespace PrintingTools.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMapper _mapper;
    private readonly ILogger<AuthService> _logger;
    private readonly JwtSettings _jwtSettings;
    
    public AuthService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMapper mapper,
        ILogger<AuthService> logger,
        IOptionsMonitor<JwtSettings> jwtSettings)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _mapper = mapper;
        _logger = logger;
        _jwtSettings = jwtSettings.CurrentValue;
    }
    
    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
         try
         {
            var user = await _unitOfWork.Users.GetByEmailWithTokensAsync(request.Email, cancellationToken);
            
            if (user == null)
            {
                _logger.LogWarning("Попытка входа с несуществующим email: {Email}", request.Email);
                return ApiResponse<LoginResponse>.Fail("Неверный email или пароль");
            }

            if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Неверный пароль для пользователя: {Email}", request.Email);
                return ApiResponse<LoginResponse>.Fail("Неверный email или пароль");
            }

            switch (user.Status)
            {
                case UserStatus.Blocked:
                    _logger.LogWarning("Попытка входа заблокированного пользователя: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.Fail("Ваш аккаунт заблокирован");
                case UserStatus.PendingVerification:
                    _logger.LogInformation("Попытка входа с неподтвержденным email: {Email}", request.Email);
                    return ApiResponse<LoginResponse>.Fail("Пожалуйста, подтвердите ваш email");
            }

            // Генерируем токены
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            // Сохраняем refresh token
            user.AddRefreshToken(refreshToken, refreshTokenExpires);
            user.UpdateLastLogin();
            
            await _unitOfWork.CompleteAsync(cancellationToken);

            var response = new LoginResponse(
                Tokens: new TokenResponse(
                    accessToken, 
                    refreshToken, 
                    DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                    TokenType: "JWT"),
                User: _mapper.Map<UserDto>(user));

            _logger.LogInformation("Успешный вход пользователя: {Email}", request.Email);
            return ApiResponse<LoginResponse>.Ok(response, "Успешный вход"); 
         }
         catch (Exception ex)
         {
            _logger.LogError(ex, "Ошибка при входе пользователя");
            return ApiResponse<LoginResponse>.Fail("Произошла ошибка при входе");
         }
    }

    public async Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _unitOfWork.Users.EmailExistsAsync(request.Email, cancellationToken))
            {
                return ApiResponse<LoginResponse>.Fail("Пользователь с таким email уже существует");
            }
         
            var passwordHash = _passwordHasher.HashPassword(request.Password);
            var user = new User(
                email: request.Email,
                passwordHash: passwordHash,
                firstName: request.FirstName,
                lastName: request.LastName,
                role: UserRole.User, 
                middleName: request.MiddleName,
                phoneNumber: request.PhoneNumber
            );

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);
            
            user.ConfirmEmail(user.EmailConfirmationToken!);
            
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            user.AddRefreshToken(refreshToken, refreshTokenExpires);
            await _unitOfWork.CompleteAsync(cancellationToken);

            var response = new LoginResponse(
                Tokens: new TokenResponse(accessToken, refreshToken, DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes), "JWT"),
                User: _mapper.Map<UserDto>(user));

            _logger.LogInformation("Зарегистрирован новый пользователь: {Email}", request.Email);
            return ApiResponse<LoginResponse>.Ok(response, "Регистрация успешна");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при регистрации пользователя");
            return ApiResponse<LoginResponse>.Fail("Произошла ошибка при регистрации");
        }
    }

    public async Task<ApiResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var (isValid, userId) = _tokenService.ValidateAccessToken(request.AccessToken);
            
            if (!userId.HasValue)
            {
                return ApiResponse<TokenResponse>.Fail("Недействительный токен");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(userId.Value);
            if (user == null)
            {
                return ApiResponse<TokenResponse>.Fail("Пользователь не найден");
            }

            var refreshToken = user.GetActiveRefreshToken(request.RefreshToken);
            if (refreshToken == null)
            {
                _logger.LogWarning("Попытка использования недействительного refresh token для пользователя {UserId}", userId);
                return ApiResponse<TokenResponse>.Fail("Недействительный refresh token");
            }

            user.RevokeRefreshToken(request.RefreshToken);

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();
            var refreshTokenExpires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays);

            user.AddRefreshToken(newRefreshToken, refreshTokenExpires);
            await _unitOfWork.CompleteAsync(cancellationToken);

            var response = new TokenResponse(
                AccessToken: newAccessToken,
                RefreshToken: newRefreshToken,
                ExpiresAt: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                TokenType: "JWT");
            

            return ApiResponse<TokenResponse>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении токена");
            return ApiResponse<TokenResponse>.Fail("Произошла ошибка при обновлении токена");
        }
    }

    public async Task<ApiResponse<object>> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("Пользователь не найден");
            }

            user.RevokeRefreshToken(refreshToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Пользователь {UserId} вышел из системы", userId);
            return ApiResponse<object>.Ok(new { }, "Выход выполнен успешно");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выходе пользователя");
            return ApiResponse<object>.Fail("Произошла ошибка при выходе");
        }
    }

    public async Task<ApiResponse<object>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return ApiResponse<object>.Fail("Пользователь не найден");
            }

            user.ConfirmEmail(request.Token);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Email подтвержден для пользователя: {Email}", request.Email);
            return ApiResponse<object>.Ok(new { }, "Email успешно подтвержден");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при подтверждении email");
            return ApiResponse<object>.Fail("Произошла ошибка при подтверждении email");
        }        
    }

    public async Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null)
            {
                return ApiResponse<object>.Ok(new { }, "Если пользователь существует, инструкции по сбросу пароля отправлены на email");
            }

            user.GeneratePasswordResetToken();
            await _unitOfWork.CompleteAsync(cancellationToken);

            // TODO: Отправить email с токеном
            _logger.LogInformation("Запрошен сброс пароля для: {Email}", request.Email);
            
            return ApiResponse<object>.Ok(new { }, "Если пользователь существует, инструкции по сбросу пароля отправлены на email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при запросе сброса пароля");
            return ApiResponse<object>.Fail("Произошла ошибка при запросе сброса пароля");
        }
    }

    public async Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
            if (user == null || !user.ValidatePasswordResetToken(request.Token))
            {
                return ApiResponse<object>.Fail("Недействительный или просроченный токен");
            }

            var passwordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatePassword(passwordHash);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Пароль сброшен для пользователя: {UserId}", user.Id);
            return ApiResponse<object>.Ok(new { }, "Пароль успешно изменен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сбросе пароля");
            return ApiResponse<object>.Fail("Произошла ошибка при сбросе пароля");
        }
    }

    public async Task<ApiResponse<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("Пользователь не найден");
            }

            if (!_passwordHasher.VerifyPassword(request.CurrentPassword, user.PasswordHash))
            {
                return ApiResponse<object>.Fail("Неверный текущий пароль");
            }

            var passwordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.UpdatePassword(passwordHash);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Пароль изменен для пользователя: {UserId}", userId);
            return ApiResponse<object>.Ok(new { }, "Пароль успешно изменен");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении пароля");
            return ApiResponse<object>.Fail("Произошла ошибка при изменении пароля");
        }
    }
}