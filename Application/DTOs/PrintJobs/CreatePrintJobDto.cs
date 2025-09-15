namespace PrintingTools.Application.DTOs.PrintJobs;

public record CreatePrintJobDto(
    IFormFile File,
    string Format,
    int Copies);