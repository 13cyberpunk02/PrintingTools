namespace PrintingTools.Application.DTOs.Auth;

public record TokenResponse(
    string AccessToken, 
    string RefreshToken,
    DateTime ExpiresAt,
    string TokenType);