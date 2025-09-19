namespace PrintingTools.Application.DTOs.PrintJobs;

public record StatisticsRequest(Guid? UserId, DateTime? From, DateTime? To);