using Microsoft.EntityFrameworkCore;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;
using PrintingTools.Infrastructure.Data;

namespace PrintingTools.Infrastructure.Repositories;

public class PrintJobRepository : Repository<PrintJob>, IPrintJobRepository
{
    public PrintJobRepository(PrintingDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<PrintJob>> GetByUserIdAsync(Guid userId, int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt);

        if (limit.HasValue)
            query = query.Take(limit.Value) as IOrderedQueryable<PrintJob>;

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<PrintJob>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
        =>  await _dbSet
            .Where(p => p.Status == PrintStatus.Pending || 
                        p.Status == PrintStatus.Queued ||
                        p.Status == PrintStatus.InProgress ||
                        p.Status == PrintStatus.Paused)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    
    public async Task<IEnumerable<PrintJob>> GetQueuedJobsAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.Status == PrintStatus.Queued)
            .OrderByDescending(p => p.Priority)
            .ThenBy(p => p.QueuedAt)
            .ToListAsync(cancellationToken);
    

    public async Task<IEnumerable<PrintJob>> GetByStatusAsync(PrintStatus status, CancellationToken cancellationToken = default)
        =>  await _dbSet
            .Where(p => p.Status == status)
            .ToListAsync(cancellationToken); 
    
    public async Task<PrintJob?> GetBySpoolJobIdAsync(string spoolJobId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Printer)
            .FirstOrDefaultAsync(p => p.SpoolJobId == spoolJobId, cancellationToken);
    
    public async Task<PrintJob?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _dbSet
            .Include(p => p.User)
            .Include(p => p.Printer)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    
    public async Task<IEnumerable<PrintJob>> GetByPrinterIdAsync(Guid printerId, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.PrinterId == printerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    

    public async Task<IEnumerable<PrintJob>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    
    public async Task<IEnumerable<PrintJob>> GetPendingJobsOrderedByPriorityAsync(CancellationToken  cancellationToken = default)
        => await _dbSet
            .Where(p => p.Status == PrintStatus.Pending)
            .OrderByDescending(p => p.Priority)
            .ThenBy(p => p.CreatedAt)
            .Take(10)
            .ToListAsync(cancellationToken);

    
    public override async Task<PrintJob?> GetByIdAsync(Guid id)
        => await _dbSet
            .Include(p => p.User)
            .Include(p => p.Printer)
            .FirstOrDefaultAsync(p => p.Id == id);
}