namespace PrintingTools.Application.DTOs.Users;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string ShortName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string Role { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool EmailConfirmed { get; set; }
    public int TotalPrintJobs { get; set; }
    public int ActivePrintJobs { get; set; }
    public int CompletedPrintJobs { get; set; }
}
    