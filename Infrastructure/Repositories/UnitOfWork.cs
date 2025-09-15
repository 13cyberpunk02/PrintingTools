using Microsoft.EntityFrameworkCore.Storage;
using PrintingTools.Infrastructure.Data;

namespace PrintingTools.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly PrintingDbContext _context;
    private IDbContextTransaction? _transaction;

    public IUserRepository Users { get; }
    public IPrintJobRepository PrintJobs { get; }
    
    public UnitOfWork(
        PrintingDbContext context,
        IUserRepository userRepository,
        IPrintJobRepository printJobRepository)
    {
        _context = context;
        Users = userRepository;
        PrintJobs = printJobRepository;
    }
    
    public async Task<int> CompleteAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
        => _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    
    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
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