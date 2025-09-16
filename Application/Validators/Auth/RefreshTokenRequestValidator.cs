using FluentValidation;
using PrintingTools.Application.DTOs.Auth;

namespace PrintingTools.Application.Validators.Auth;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Токен обновления обязателен");
        RuleFor(x => x.AccessToken)
            .NotEmpty().WithMessage("Токен обязателен");
    }
}