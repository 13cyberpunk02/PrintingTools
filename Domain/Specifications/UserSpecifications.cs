using System.Linq.Expressions;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Specifications;

public static class UserSpecifications
{
    public static Expression<Func<User, bool>> IsActive()
    {
        return user => user.Status == UserStatus.Active && 
                       user.EmailConfirmedAt != null &&
                       !user.IsDeleted;
    }

    public static Expression<Func<User, bool>> ByEmail(string email)
    {
        var normalizedEmail = email.ToLowerInvariant();
        return user => user.Email == normalizedEmail;
    }

    public static Expression<Func<User, bool>> ByRole(UserRole role)
    {
        return user => user.Role == role;
    }

    public static Expression<Func<User, bool>> Administrators()
    {
        return user => user.Role == UserRole.Administrator;
    }

    public static Expression<Func<User, bool>> RegularUsers()
    {
        return user => user.Role == UserRole.User;
    }

    public static Expression<Func<User, bool>> CanLogin()
    {
        return user => (user.Status == UserStatus.Active || 
                        user.Status == UserStatus.PendingVerification) &&
                       !user.IsDeleted;
    }

    public static Expression<Func<User, bool>> NeedsEmailConfirmation()
    {
        return user => user.Status == UserStatus.PendingVerification && 
                       user.EmailConfirmedAt == null;
    }

    public static Expression<Func<User, bool>> Blocked()
    {
        return user => user.Status == UserStatus.Blocked;
    }
}