namespace PrintingTools.Application.DTOs.Auth;

public record RegisterRequest(
    string Email, 
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber);