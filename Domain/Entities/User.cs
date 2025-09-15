using PrintingTools.Domain.Common;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Entities;

public class User : BaseEntity
{
    private readonly List<RefreshToken> _refreshTokens = new();

    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string? MiddleName { get; private set; }
    public string? PhoneNumber { get; private set; }
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? EmailConfirmedAt { get; private set; }
    public string? EmailConfirmationToken { get; private set; }
    public string? PasswordResetToken { get; private set; }
    public DateTime? PasswordResetTokenExpires { get; private set; }
    
    public IReadOnlyCollection<RefreshToken> RefreshTokens => _refreshTokens.AsReadOnly();
    
    protected User() { }
    
    public User(
        string email,
        string passwordHash,
        string firstName,
        string lastName,
        UserRole role = UserRole.User,
        string? middleName = null,
        string? phoneNumber = null)
    {
        SetEmail(email);
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        SetName(firstName, lastName, middleName);
        Role = role;
        PhoneNumber = phoneNumber;
        Status = UserStatus.PendingVerification;
        GenerateEmailConfirmationToken();
    }
    
    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email не может быть пустым", nameof(email));
        
        if (!email.Contains('@'))
            throw new ArgumentException("Некорректный формат email", nameof(email));
        
        Email = email.ToLowerInvariant();
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void SetName(string firstName, string lastName, string? middleName = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Имя не может быть пустым", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Фамилия не может быть пустой", nameof(lastName));
        
        FirstName = firstName;
        LastName = lastName;
        MiddleName = middleName;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        PasswordResetToken = null;
        PasswordResetTokenExpires = null;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePhoneNumber(string? phoneNumber)
    {
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateLastLogin()
    {
        LastLoginAt = DateTime.UtcNow;
    }

    public void ConfirmEmail(string token)
    {
        if (EmailConfirmationToken != token)
            throw new InvalidOperationException("Неверный токен подтверждения");
        
        EmailConfirmedAt = DateTime.UtcNow;
        EmailConfirmationToken = null;
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void GenerateEmailConfirmationToken()
    {
        EmailConfirmationToken = Guid.NewGuid().ToString("N");
        UpdatedAt = DateTime.UtcNow;
    }

    public void GeneratePasswordResetToken()
    {
        PasswordResetToken = Guid.NewGuid().ToString("N");
        PasswordResetTokenExpires = DateTime.UtcNow.AddHours(24);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool ValidatePasswordResetToken(string token)
    {
        return PasswordResetToken == token && 
               PasswordResetTokenExpires > DateTime.UtcNow;
    }

    public void Activate()
    {
        if (EmailConfirmedAt == null)
            throw new InvalidOperationException("Email должен быть подтвержден перед активацией");
        
        Status = UserStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Block()
    {
        Status = UserStatus.Blocked;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddRefreshToken(string token, DateTime expires)
    {
        _refreshTokens.Add(new RefreshToken(token, expires));
        
        // Удаляем старые токены (храним максимум 5)
        var tokensToRemove = _refreshTokens
            .OrderByDescending(t => t.Created)
            .Skip(5)
            .ToList();
        
        foreach (var oldToken in tokensToRemove)
        {
            _refreshTokens.Remove(oldToken);
        }
    }

    public void RevokeAllRefreshTokens()
    {
        foreach (var token in _refreshTokens)
        {
            token.Revoke();
        }
    }

    public void RevokeRefreshToken(string token)
    {
        var refreshToken = _refreshTokens.FirstOrDefault(t => t.Token == token);
        refreshToken?.Revoke();
    }

    public RefreshToken? GetActiveRefreshToken(string token)
    {
        return _refreshTokens.FirstOrDefault(t => t.Token == token && t.IsActive);
    }

    public string GetFullName()
    {
        var parts = new List<string> { LastName, FirstName };
        if (!string.IsNullOrWhiteSpace(MiddleName))
            parts.Add(MiddleName);
        
        return string.Join(" ", parts);
    }

    public string GetShortName()
    {
        var middleInitial = !string.IsNullOrWhiteSpace(MiddleName) ? $" {MiddleName[0]}." : "";
        return $"{LastName} {FirstName[0]}.{middleInitial}";
    }

    public bool IsAdmin() => Role == UserRole.Administrator;
    
    public bool CanPrint() => Status == UserStatus.Active && EmailConfirmedAt != null;
    
    public bool CanEditProfile(Guid targetUserId) => 
        Id == targetUserId || Role == UserRole.Administrator;
}