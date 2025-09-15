using FluentValidation;
using PrintingTools.Application.DTOs.PrintJobs;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Application.Validators.PrintJobs;

public class UpdatePrintJobStatusDtoValidator : AbstractValidator<UpdatePrintJobStatusDto>
{
    public UpdatePrintJobStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Статус обязателен")
            .Must(BeValidStatus).WithMessage("Некорректный статус");

        RuleFor(x => x.PrinterName)
            .MaximumLength(200).WithMessage("Название принтера слишком длинное")
            .When(x => !string.IsNullOrEmpty(x.PrinterName));

        RuleFor(x => x.ErrorMessage)
            .MaximumLength(1000).WithMessage("Сообщение об ошибке слишком длинное")
            .When(x => !string.IsNullOrEmpty(x.ErrorMessage));

        RuleFor(x => x.Cost)
            .GreaterThanOrEqualTo(0).WithMessage("Стоимость не может быть отрицательной")
            .When(x => x.Cost.HasValue);
    }

    private bool BeValidStatus(string status)
    {
        return Enum.TryParse<PrintStatus>(status, true, out _);
    }
}