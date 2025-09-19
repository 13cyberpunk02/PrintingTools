namespace PrintingTools.Application.DTOs.PrintJobs;

public class PrintJobStatisticsDto
{
    public int TotalJobs { get; set; }
    public int PendingJobs { get; set; }
    public int QueuedJobs { get; set; }
    public int InProgressJobs { get; set; }
    public int CompletedJobs { get; set; }
    public int FailedJobs { get; set; }
    public int CancelledJobs { get; set; }
    public long TotalPrintedPages { get; set; }
    public double AverageJobDurationMinutes { get; set; }
    public double SuccessRate { get; set; }
    public Dictionary<string, int> JobsByFormat { get; set; } = new();
    public Dictionary<string, int> JobsByFileType { get; set; } = new();
    public List<DailyStatistics> DailyStats { get; set; } = [];
    public List<PrinterStatistics> PrinterStats { get; set; } = [];
}