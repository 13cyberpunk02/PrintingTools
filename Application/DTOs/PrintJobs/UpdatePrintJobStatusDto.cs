namespace PrintingTools.Application.DTOs.PrintJobs;

public class UpdatePrintJobStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? PrinterName { get; set; }
    public string? ErrorMessage { get; set; }
}