using FluentValidation;
using PrintingTools.Application.DTOs.Users;

namespace PrintingTools.Application.Validators.Users;

public class UpdateUserDtoValidator : AbstractValidator<UpdateUserDto>
{
    public UpdateUserDtoValidator()
    {
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