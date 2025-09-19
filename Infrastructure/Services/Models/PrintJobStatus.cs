namespace PrintingTools.Infrastructure.Services.Models;

public class PrintJobStatus
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int TotalPages { get; set; }
    public int PrintedPages { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EstimatedCompletionTime { get; set; }
}