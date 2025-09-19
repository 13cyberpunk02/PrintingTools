using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Infrastructure.Repositories;

public interface IPrintJobRepository : IRepository<PrintJob>
{
    Task<IEnumerable<PrintJob>> GetByUserIdAsync(Guid userId, int? limit = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetActiveJobsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetQueuedJobsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetByStatusAsync(PrintStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetByPrinterIdAsync(Guid printerId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<PrintJob?> GetBySpoolJobIdAsync(string spoolJobId, CancellationToken cancellationToken = default);
    Task<PrintJob?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetPendingJobsOrderedByPriorityAsync(CancellationToken cancellationToken = default);
}