using FluentValidation;
using PrintingTools.Application.DTOs.Auth;

namespace PrintingTools.Application.Validators.Auth;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{ 
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Текущий пароль обязателен");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Новый пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов")
            .MaximumLength(100).WithMessage("Пароль слишком длинный")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>]").WithMessage("Пароль должен содержать хотя бы один специальный символ")
            .NotEqual(x => x.CurrentPassword).WithMessage("Новый пароль должен отличаться от текущего");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Подтверждение пароля обязательно")
            .Equal(x => x.NewPassword).WithMessage("Пароли не совпадают");
    }
}