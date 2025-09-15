namespace PrintingTools.Application.DTOs.PrintJobs;

public record DailyStatistics(DateTime Date, int JobCount, decimal TotalCost);