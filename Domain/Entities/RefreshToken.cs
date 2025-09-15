namespace PrintingTools.Domain.Entities;

public class RefreshToken
{
    public string Token { get; private set; }
    public DateTime Expires { get; private set; }
    public DateTime Created { get; private set; }
    public DateTime? Revoked { get; private set; }
    public string? RevokedByIp { get; private set; }
    public string? ReplacedByToken { get; private set; }
    
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsRevoked => Revoked != null;
    public bool IsActive => !IsRevoked && !IsExpired;

    protected RefreshToken() { } // Для EF Core

    public RefreshToken(string token, DateTime expires)
    {
        Token = token ?? throw new ArgumentNullException(nameof(token));
        Expires = expires;
        Created = DateTime.UtcNow;
    }

    public void Revoke(string? byIp = null, string? replacedByToken = null)
    {
        Revoked = DateTime.UtcNow;
        RevokedByIp = byIp;
        ReplacedByToken = replacedByToken;
    }
}