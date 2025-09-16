using PrintingTools.Application.DTOs.Common;
using PrintingTools.Application.DTOs.Users;

namespace PrintingTools.Application.Services;

public interface IUserService
{
    Task<ApiResponse<UserProfileDto>> GetProfileAsync(Guid userId);
    Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId);
    Task<ApiResponse<UsersListDto>> GetUsersAsync(PagedRequest request);
    Task<ApiResponse<UserDto>> CreateUserAsync(CreateUserDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> UpdateUserRoleAsync(Guid userId, UpdateUserRoleDto dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> ActivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> DeactivateUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> BlockUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<ApiResponse<object>> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
}