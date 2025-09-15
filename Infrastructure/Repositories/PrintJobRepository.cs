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

    public async Task<IEnumerable<PrintJob>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<PrintJob>> GetActiveJobsAsync(CancellationToken cancellationToken = default)
        =>  await _dbSet
            .Where(p => p.Status == PrintStatus.Pending || p.Status == PrintStatus.InProgress)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<PrintJob>> GetByStatusAsync(PrintStatus status, CancellationToken cancellationToken = default)
        =>  await _dbSet
            .Where(p => p.Status == status)
            .ToListAsync(cancellationToken); 

    public async Task<IEnumerable<PrintJob>> GetByDateRangeAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    
    public override async Task<PrintJob?> GetByIdAsync(Guid id)
        => await  _dbSet
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);
}