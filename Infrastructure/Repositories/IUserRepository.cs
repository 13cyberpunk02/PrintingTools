using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Infrastructure.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailWithTokensAsync(string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetByRoleAsync(UserRole role,  CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default);
}