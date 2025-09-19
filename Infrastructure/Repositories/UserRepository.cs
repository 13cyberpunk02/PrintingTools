using Microsoft.EntityFrameworkCore;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;
using PrintingTools.Infrastructure.Data;

namespace PrintingTools.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(PrintingDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _dbSet.FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<User?> GetByEmailWithTokensAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _dbSet
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);    
    }

    public async Task<IEnumerable<User>> GetByRoleAsync(UserRole role, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(u => u.Role == role)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return await _dbSet.AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            u => u.PasswordResetToken == token && 
                 u.PasswordResetTokenExpires > DateTime.UtcNow, cancellationToken);
    }

    public override async Task<User?> GetByIdAsync(Guid id)
        => await _dbSet
            .Include(u => u.RefreshTokens)
            .FirstOrDefaultAsync(u => u.Id == id);
    
    public async Task<User?> GetByEmailConfirmationTokenAsync(string token, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(u => u.EmailConfirmationToken == token, cancellationToken);

    public async Task<IEnumerable<User>> GetActiveUsersAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(u => u.Status == UserStatus.Active && u.EmailConfirmedAt != null)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);
}