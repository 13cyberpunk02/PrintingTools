using FluentValidation;
using PrintingTools.Application.DTOs.Auth;

namespace PrintingTools.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email")
            .MaximumLength(256).WithMessage("Email слишком длинный");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов")
            .MaximumLength(100).WithMessage("Пароль слишком длинный")
            .Matches(@"[A-Z]").WithMessage("Пароль должен содержать хотя бы одну заглавную букву")
            .Matches(@"[a-z]").WithMessage("Пароль должен содержать хотя бы одну строчную букву")
            .Matches(@"[0-9]").WithMessage("Пароль должен содержать хотя бы одну цифру")
            .Matches(@"[!@#$%^&*(),.?"":{}|<>]").WithMessage("Пароль должен содержать хотя бы один специальный символ");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Подтверждение пароля обязательно")
            .Equal(x => x.Password).WithMessage("Пароли не совпадают");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .MaximumLength(100).WithMessage("Имя слишком длинное")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z\s-]+$").WithMessage("Имя содержит недопустимые символы");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .MaximumLength(100).WithMessage("Фамилия слишком длинная")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z\s-]+$").WithMessage("Фамилия содержит недопустимые символы");

        RuleFor(x => x.MiddleName)
            .MaximumLength(100).WithMessage("Отчество слишком длинное")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z\s-]*$").WithMessage("Отчество содержит недопустимые символы")
            .When(x => !string.IsNullOrEmpty(x.MiddleName));

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Некорректный формат телефона")
            .MinimumLength(10).WithMessage("Телефон слишком короткий")
            .MaximumLength(20).WithMessage("Телефон слишком длинный")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}