using PrintingTools.Domain.Entities;

namespace PrintingTools.Domain.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    (bool IsValid, Guid? UserId) ValidateAccessToken(string token);
}