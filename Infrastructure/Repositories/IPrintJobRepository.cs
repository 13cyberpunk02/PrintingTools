using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Infrastructure.Repositories;

public interface IPrintJobRepository : IRepository<PrintJob>
{
    Task<IEnumerable<PrintJob>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetActiveJobsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetByStatusAsync(PrintStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<PrintJob>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}