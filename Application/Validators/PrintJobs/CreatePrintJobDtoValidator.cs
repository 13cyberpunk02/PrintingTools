using FluentValidation;
using PrintingTools.Application.DTOs.PrintJobs;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Application.Validators.PrintJobs;

public class CreatePrintJobDtoValidator : AbstractValidator<CreatePrintJobDto>
{
    private const long MaxFileSize = 100 * 1024 * 1024; // 100 MB
    private readonly string[] _allowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png", ".tiff", ".bmp" };

    public CreatePrintJobDtoValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Файл обязателен")
            .Must(BeValidFile).WithMessage("Недопустимый формат файла. Разрешены: PDF, JPG, PNG, TIFF, BMP")
            .Must(BeValidSize).WithMessage($"Размер файла не должен превышать {MaxFileSize / (1024 * 1024)} MB");

        RuleFor(x => x.Format)
            .NotEmpty().WithMessage("Формат печати обязателен")
            .Must(BeValidFormat).WithMessage("Некорректный формат печати");

        RuleFor(x => x.Copies)
            .GreaterThan(0).WithMessage("Количество копий должно быть больше 0")
            .LessThanOrEqualTo(100).WithMessage("Максимальное количество копий - 100");
    }

    private bool BeValidFile(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return false;
        
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return _allowedExtensions.Contains(extension);
    }

    private bool BeValidSize(Microsoft.AspNetCore.Http.IFormFile? file)
    {
        return file != null && file.Length <= MaxFileSize;
    }

    private bool BeValidFormat(string format)
    {
        return Enum.TryParse<PrintFormat>(format, true, out _);
    }
}