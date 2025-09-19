using Microsoft.EntityFrameworkCore;
using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;
using PrintingTools.Infrastructure.Data;

namespace PrintingTools.Infrastructure.Repositories;

public class PrinterRepository : Repository<Printer>, IPrinterRepository
{
    public PrinterRepository(PrintingDbContext context) : base(context)
    {
    }

    public async Task<List<Printer>> GetAvailablePrintersAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.Status == PrinterStatus.Online || p.Status == PrinterStatus.Busy)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<Printer?> GetDefaultPrinterAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsDefault, cancellationToken);

    public async Task<List<Printer>> GetPrintersByTypeAsync(PrinterType type, CancellationToken cancellationToken = default)
        => await _dbSet
            .Where(p => p.Type == type)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<Printer?> GetPrinterByNameAsync(string name, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

    public async Task<Printer?> GetPrinterByNetworkPathAsync(string networkPath, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .FirstOrDefaultAsync(p => p.NetworkPath == networkPath, cancellationToken);

    public async Task<List<Printer>> GetOnlinePrintersAsync(CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.Status == PrinterStatus.Online)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);

    public async Task<List<Printer>> GetPrintersNeedingMaintenanceAsync(int pageThreshold = 10000, CancellationToken cancellationToken = default)
        => await _dbSet.AsNoTracking()
            .Where(p => p.PagesPrinted > pageThreshold || 
                        p.LastMaintenanceDate == null ||
                        p.LastMaintenanceDate < DateTime.UtcNow.AddDays(-90))
            .OrderByDescending(p => p.PagesPrinted)
            .ToListAsync(cancellationToken);
}