namespace PrintingTools.Application.DTOs.PrintJobs;

public record PrintJobDto(
    Guid Id,
    Guid UserId,
    string UserName,
    string FileName,
    long FileSizeBytes,
    string FileSizeFormatted,
    string Format,
    int Copies,
    string Status,
    string? PrinterName,
    DateTime CreatedAt,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? ErrorMessage,
    decimal? Cost);