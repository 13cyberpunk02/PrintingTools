using PrintingTools.Domain.Entities;
using PrintingTools.Domain.ValueObjects;

namespace PrintingTools.Infrastructure.Repositories;

public interface IPrinterRepository : IRepository<Printer>
{
    Task<List<Printer>> GetAvailablePrintersAsync(CancellationToken cancellationToken = default);
    Task<Printer?> GetDefaultPrinterAsync(CancellationToken cancellationToken = default);
    Task<List<Printer>> GetPrintersByTypeAsync(PrinterType type, CancellationToken cancellationToken = default);
    Task<Printer?> GetPrinterByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<Printer?> GetPrinterByNetworkPathAsync(string networkPath, CancellationToken cancellationToken = default);
    Task<List<Printer>> GetOnlinePrintersAsync(CancellationToken cancellationToken = default);
    Task<List<Printer>> GetPrintersNeedingMaintenanceAsync(int pageThreshold = 10000, CancellationToken cancellationToken = default);
}