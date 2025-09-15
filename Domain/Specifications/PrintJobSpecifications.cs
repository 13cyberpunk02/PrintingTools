using System.Linq.Expressions;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Domain.Specifications;

public class PrintJobSpecifications
{
    public static Expression<Func<PrintJob, bool>> ByUser(Guid userId)
    {
        return job => job.UserId == userId;
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
                      job.Status == PrintStatus.InProgress;
    }
}