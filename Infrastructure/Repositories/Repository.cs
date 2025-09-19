using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PrintingTools.Domain.Common;
using PrintingTools.Infrastructure.Data;

namespace PrintingTools.Infrastructure.Repositories;

public class Repository<T>  : IRepository<T> where T : BaseEntity
{
    protected readonly PrintingDbContext _context;
    protected readonly DbSet<T> _dbSet;
    
    public Repository(PrintingDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }
    
    public virtual async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken) 
        => await _dbSet.AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken) 
        => await _dbSet.Where(predicate).AsNoTracking().ToListAsync(cancellationToken);

    public virtual async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    => await _dbSet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity)
    {
        entity.Delete();
        _dbSet.Remove(entity);
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.CountAsync(predicate, cancellationToken);
    
    public virtual async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.AnyAsync(predicate, cancellationToken);
    
    public virtual IQueryable<T> Query() => _dbSet.AsQueryable();
    
}