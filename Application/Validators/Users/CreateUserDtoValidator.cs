using FluentValidation;
using PrintingTools.Application.DTOs.Users;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Application.Validators.Users;

public class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email обязателен")
            .EmailAddress().WithMessage("Некорректный формат email")
            .MaximumLength(256).WithMessage("Email слишком длинный");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Пароль обязателен")
            .MinimumLength(8).WithMessage("Пароль должен содержать минимум 8 символов")
            .MaximumLength(100).WithMessage("Пароль слишком длинный");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Имя обязательно")
            .MaximumLength(100).WithMessage("Имя слишком длинное")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z\s-]+$").WithMessage("Имя содержит недопустимые символы");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Фамилия обязательна")
            .MaximumLength(100).WithMessage("Фамилия слишком длинная")
            .Matches(@"^[а-яА-ЯёЁa-zA-Z\s-]+$").WithMessage("Фамилия содержит недопустимые символы");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Роль обязательна")
            .Must(BeValidRole).WithMessage("Некорректная роль");
    }

    private bool BeValidRole(string role)
    {
        return Enum.TryParse<UserRole>(role, true, out _);
    }
}