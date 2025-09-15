namespace PrintingTools.Application.DTOs.Users;

public record CreateUserDto(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? MiddleName,
    string? PhoneNumber,
    string Role = "User");