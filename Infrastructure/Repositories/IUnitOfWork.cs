namespace PrintingTools.Infrastructure.Repositories;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    IPrintJobRepository PrintJobs { get; }
    IPrinterRepository Printers { get; }
    Task<int> CompleteAsync();
    Task<int> CompleteAsync(CancellationToken cancellationToken);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    bool HasActiveTransaction { get; }
}