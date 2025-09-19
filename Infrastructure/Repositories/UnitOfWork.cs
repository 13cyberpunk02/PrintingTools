using Microsoft.EntityFrameworkCore.Storage;
using PrintingTools.Infrastructure.Data;

namespace PrintingTools.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly PrintingDbContext _context;
    private IDbContextTransaction? _transaction;
    
    public IUserRepository Users { get; }
    public IPrintJobRepository PrintJobs { get; }
    public IPrinterRepository Printers { get; }

    public UnitOfWork(
        PrintingDbContext context,
        IUserRepository userRepository,
        IPrintJobRepository printJobRepository,
        IPrinterRepository printerRepository)
    {
        _context = context;
        Users = userRepository;
        PrintJobs = printJobRepository;
        Printers = printerRepository;
    }

    public bool HasActiveTransaction => _transaction != null;

    public async Task<int> CompleteAsync()
        => await _context.SaveChangesAsync();

    public async Task<int> CompleteAsync(CancellationToken cancellationToken)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("Транзакция уже начато");
        }
        
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Нет активных транзакция для коммита.");
        }

        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to rollback");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}