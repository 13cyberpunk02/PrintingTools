namespace PrintingTools.Application.DTOs.Users;

public record UpdateUserDto(string FirstName, string LastName, string? MiddleName, string? PhoneNumber);