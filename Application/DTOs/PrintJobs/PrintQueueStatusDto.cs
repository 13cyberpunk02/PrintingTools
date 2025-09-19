namespace PrintingTools.Application.DTOs.PrintJobs;

public class PrintQueueStatusDto
{
    public int TotalJobs { get; set; }
    public int PendingJobs { get; set; }
    public int ProcessingJobs { get; set; }
    public List<QueuedJobDto> Jobs { get; set; } = [];
}