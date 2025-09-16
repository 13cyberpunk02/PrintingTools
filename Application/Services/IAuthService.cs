using PrintingTools.Application.DTOs.Auth;
using PrintingTools.Application.DTOs.Common;

namespace PrintingTools.Application.Services;

public interface IAuthService
{
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<LoginResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<TokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> LogoutAsync(Guid userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ConfirmEmailAsync(ConfirmEmailRequest request, CancellationToken  cancellationToken = default);
    Task<ApiResponse<object>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);
}