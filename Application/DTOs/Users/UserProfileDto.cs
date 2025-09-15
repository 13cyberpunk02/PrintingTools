namespace PrintingTools.Application.DTOs.Users;

public record UserProfileDto(
    Guid Id,
    string Email, 
    string FirstName,
    string LastName,
    string? MiddleName,
    string FullName,
    string? PhoneNumber,
    string Role,
    DateTime CreatedAt,
    DateTime? LastLoginAt,
    bool EmailConfirmed,
    int TotalPrintJobs,
    int ActivePrintJobs);