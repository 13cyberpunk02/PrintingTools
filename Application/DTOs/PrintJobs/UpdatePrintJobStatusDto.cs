namespace PrintingTools.Application.DTOs.PrintJobs;

public record UpdatePrintJobStatusDto(string Status, string? PrinterName, string? ErrorMessage, decimal? Cost);