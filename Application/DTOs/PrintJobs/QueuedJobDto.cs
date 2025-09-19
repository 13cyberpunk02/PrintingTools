namespace PrintingTools.Application.DTOs.PrintJobs;

public class QueuedJobDto
{
    public Guid JobId { get; set; }
    public int Position { get; set; }
    public string Status { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public DateTime EnqueuedAt { get; set; }
    public string EstimatedStartTime { get; set; } = string.Empty;
}