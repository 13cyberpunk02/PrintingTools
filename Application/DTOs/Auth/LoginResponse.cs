using PrintingTools.Application.DTOs.Users;

namespace PrintingTools.Application.DTOs.Auth;

public record LoginResponse(TokenResponse Tokens, UserDto User);