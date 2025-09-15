namespace PrintingTools.Application.DTOs.Auth;

public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmNewPassword);