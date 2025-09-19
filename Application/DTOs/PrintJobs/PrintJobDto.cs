namespace PrintingTools.Application.DTOs.PrintJobs;

public class PrintJobDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public Guid? PrinterId { get; set; }
    public string? PrinterName { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public string FileType { get; set; } = string.Empty;
    public string Format { get; set; } = string.Empty;
    public int Copies { get; set; }
    public bool IsColor { get; set; }
    public bool IsDuplex { get; set; }
    public string Quality { get; set; } = string.Empty;
    public int Priority { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? TotalPages { get; set; }
    public int? PrintedPages { get; set; }
    public int? ProgressPercentage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? QueuedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
    public string? EstimatedCompletionTime { get; set; }
    public TimeSpan? PrintDuration { get; set; }
    
    public bool CanCancel { get; set; }
    public bool CanPause { get; set; }
    public bool CanResume { get; set; }
}