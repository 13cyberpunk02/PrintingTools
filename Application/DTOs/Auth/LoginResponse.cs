using PrintingTools.Application.DTOs.Users;

namespace PrintingTools.Application.DTOs.Auth;

public class LoginResponse
{
    public TokenResponse Tokens { get; set; } = null!;
    public UserDto User { get; set; } = null!;
}