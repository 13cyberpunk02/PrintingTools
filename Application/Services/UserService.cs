using AutoMapper;
using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.Users;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.Services;
using PrintingTools.Domain.ValueObjects;
using PrintingTools.Infrastructure.Repositories;

namespace PrintingTools.Application.Services;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IMapper _mapper;
    private readonly ILogger<UserService> _logger;
    
    public UserService(
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IMapper mapper,
        ILogger<UserService> logger)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _logger = logger;
    }
    
    public async Task<ApiResponse<UserProfileDto>> GetProfileAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserProfileDto>.Fail("Пользователь не найден");
            }

            var profile = _mapper.Map<UserProfileDto>(user);
            
            // Добавляем статистику по заданиям печати
            var userJobs = await _unitOfWork.PrintJobs.GetByUserIdAsync(userId);
            var printJobs = userJobs.ToList();
            profile.TotalPrintJobs = printJobs.Count;
            profile.ActivePrintJobs = printJobs.Count(j => 
                j.Status is PrintStatus.Pending or PrintStatus.InProgress);

            return ApiResponse<UserProfileDto>.Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении профиля пользователя {UserId}", userId);
            return ApiResponse<UserProfileDto>.Fail("Произошла ошибка при получении профиля");
        }
    }

    public async Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.Fail("Пользователь не найден");
            }

            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto>.Ok(userDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении пользователя {UserId}", userId);
            return ApiResponse<UserDto>.Fail("Произошла ошибка при получении пользователя");
        }
    }

    public async Task<ApiResponse<UsersListDto>> GetUsersAsync(PagedRequest request)
    {
        try
        {
            var allUsers = await _unitOfWork.Users.GetAllAsync();
            
            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                var searchTerm = request.SearchTerm.ToLower();
                allUsers = allUsers.Where(u => 
                    u.Email.ToLower().Contains(searchTerm) ||
                    u.FirstName.ToLower().Contains(searchTerm) ||
                    u.LastName.ToLower().Contains(searchTerm));
            }

            allUsers = request.SortBy?.ToLower() switch
            {
                "email" => request.SortDescending ? 
                    allUsers.OrderByDescending(u => u.Email) : 
                    allUsers.OrderBy(u => u.Email),
                "name" => request.SortDescending ? 
                    allUsers.OrderByDescending(u => u.LastName).ThenByDescending(u => u.FirstName) : 
                    allUsers.OrderBy(u => u.LastName).ThenBy(u => u.FirstName),
                "createdat" => request.SortDescending ? 
                    allUsers.OrderByDescending(u => u.CreatedAt) : 
                    allUsers.OrderBy(u => u.CreatedAt),
                _ => allUsers.OrderByDescending(u => u.CreatedAt)
            };

            var totalCount = allUsers.Count();
            var skip = (request.PageNumber - 1) * request.PageSize;
            var pagedUsers = allUsers.Skip(skip).Take(request.PageSize).ToList();

            var response = new UsersListDto
            {
                Users = _mapper.Map<List<UserDto>>(pagedUsers),
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            return ApiResponse<UsersListDto>.Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении списка пользователей");
            return ApiResponse<UsersListDto>.Fail("Произошла ошибка при получении списка пользователей");
        }
    }

    public async Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (await _unitOfWork.Users.EmailExistsAsync(dto.Email, cancellationToken))
            {
                return ApiResponse<UserDto>.Fail("Пользователь с таким email уже существует");
            }

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            {
                return ApiResponse<UserDto>.Fail("Некорректная роль");
            }

            var passwordHash = _passwordHasher.HashPassword(dto.Password);
            var user = new User(
                email: dto.Email,
                passwordHash: passwordHash,
                firstName: dto.FirstName,
                lastName: dto.LastName,
                role: role,
                middleName: dto.MiddleName,
                phoneNumber: dto.PhoneNumber
            );

            user.ConfirmEmail(user.EmailConfirmationToken!);

            await _unitOfWork.Users.AddAsync(user, cancellationToken);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Создан новый пользователь: {Email} с ролью {Role}", dto.Email, dto.Role);
            
            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto>.Ok(userDto, "Пользователь успешно создан");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании пользователя");
            return ApiResponse<UserDto>.Fail("Произошла ошибка при создании пользователя");
        }
    }

    public async Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserDto>.Fail("Пользователь не найден");
            }

            user.SetName(dto.FirstName, dto.LastName, dto.MiddleName);
            user.UpdatePhoneNumber(dto.PhoneNumber);

            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Обновлены данные пользователя: {UserId}", userId);
            
            var userDto = _mapper.Map<UserDto>(user);
            return ApiResponse<UserDto>.Ok(userDto, "Данные пользователя обновлены");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обновлении пользователя {UserId}", userId);
            return ApiResponse<UserDto>.Fail("Произошла ошибка при обновлении пользователя");
        }
    }

    public async Task<ApiResponse<object>> UpdateUserRoleAsync(Guid userId, UpdateUserRoleDto dto, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("Пользователь не найден");
            }

            if (!Enum.TryParse<UserRole>(dto.Role, true, out var role))
            {
                return ApiResponse<object>.Fail("Некорректная роль");
            }

            user.ChangeRole(role);
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Изменена роль пользователя {UserId} на {Role}", userId, role);
            return ApiResponse<object>.Ok(new { }, "Роль пользователя изменена");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при изменении роли пользователя {UserId}", userId);
            return ApiResponse<object>.Fail("Произошла ошибка при изменении роли");
        }
    }

    public async Task<ApiResponse<object>> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("Пользователь не найден");
            }

            user.Activate();
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Активирован пользователь: {UserId}", userId);
            return ApiResponse<object>.Ok(new { }, "Пользователь активирован");
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<object>.Fail(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при активации пользователя {UserId}", userId);
            return ApiResponse<object>.Fail("Произошла ошибка при активации пользователя");
        }
    }

    public async Task<ApiResponse<object>> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("Пользователь не найден");
            }

            user.Deactivate();
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Деактивирован пользователь: {UserId}", userId);
            return ApiResponse<object>.Ok(new { }, "Пользователь деактивирован");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при деактивации пользователя {UserId}", userId);
            return ApiResponse<object>.Fail("Произошла ошибка при деактивации пользователя");
        }
    }

    public async Task<ApiResponse<object>> BlockUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("Пользователь не найден");
            }

            user.Block();
            user.RevokeAllRefreshTokens();
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Заблокирован пользователь: {UserId}", userId);
            return ApiResponse<object>.Ok(new { }, "Пользователь заблокирован");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при блокировке пользователя {UserId}", userId);
            return ApiResponse<object>.Fail("Произошла ошибка при блокировке пользователя");
        }
    }

    public async Task<ApiResponse<object>> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<object>.Fail("Пользователь не найден");
            }
            
            var activeJobs = await _unitOfWork.PrintJobs.GetByUserIdAsync(userId, 10, cancellationToken);
            if (activeJobs.Any(j => j.Status is PrintStatus.Pending or PrintStatus.InProgress))
            {
                return ApiResponse<object>.Fail("Невозможно удалить пользователя с активными заданиями печати");
            }

            user.Delete(); // Soft delete
            user.RevokeAllRefreshTokens();
            await _unitOfWork.CompleteAsync(cancellationToken);

            _logger.LogInformation("Удален пользователь: {UserId}", userId);
            return ApiResponse<object>.Ok(new { }, "Пользователь удален");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при удалении пользователя {UserId}", userId);
            return ApiResponse<object>.Fail("Произошла ошибка при удалении пользователя");
        }
    }
}