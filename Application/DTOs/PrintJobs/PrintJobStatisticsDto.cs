namespace PrintingTools.Application.DTOs.PrintJobs;

public record PrintJobStatisticsDto(int TotalJobs,
    int PendingJobs, 
    int InProgressJobs,
    int CompletedJobs, 
    int FailedJobs, 
    int CancelledJobs,
    decimal TotalCost,
    long TotalPrintedPages,
    Dictionary<string, int> JobsByFormat,
    List<DailyStatistics> DailyStats);