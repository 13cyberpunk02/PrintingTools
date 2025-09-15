namespace PrintingTools.Application.DTOs.Auth;

public record ResetPasswordRequest(
    string Token,
    string NewPassword,
    string ConfirmPassword);