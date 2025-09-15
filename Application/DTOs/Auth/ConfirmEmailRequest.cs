namespace PrintingTools.Application.DTOs.Auth;

public record ConfirmEmailRequest(string Email, string Token);