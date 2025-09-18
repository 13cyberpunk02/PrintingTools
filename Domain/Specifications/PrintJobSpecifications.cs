using System.Linq.Expressions;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Specifications;

public static class PrintJobSpecifications
{
    public static Expression<Func<PrintJob, bool>> ByUser(Guid userId)
    {
        return job => job.UserId == userId;
    }

    public static Expression<Func<PrintJob, bool>> ByPrinter(Guid printerId)
    {
        return job => job.PrinterId == printerId;
    }

    public static Expression<Func<PrintJob, bool>> ByStatus(PrintStatus status)
    {
        return job => job.Status == status;
    }

    public static Expression<Func<PrintJob, bool>> InDateRange(DateTime from, DateTime to)
    {
        return job => job.CreatedAt >= from && job.CreatedAt <= to;
    }

    public static Expression<Func<PrintJob, bool>> Active()
    {
        return job => job.Status == PrintStatus.Pending || 
                      job.Status == PrintStatus.Queued ||
                      job.Status == PrintStatus.InProgress ||
                      job.Status == PrintStatus.Paused;
    }

    public static Expression<Func<PrintJob, bool>> Completed()
    {
        return job => job.Status == PrintStatus.Completed;
    }

    public static Expression<Func<PrintJob, bool>> Failed()
    {
        return job => job.Status == PrintStatus.Failed;
    }

    public static Expression<Func<PrintJob, bool>> HighPriority()
    {
        return job => job.Priority >= 8;
    }

    public static Expression<Func<PrintJob, bool>> ByFileType(string fileType)
    {
        var normalizedType = fileType.ToLowerInvariant();
        return job => job.FileType == normalizedType;
    }
}