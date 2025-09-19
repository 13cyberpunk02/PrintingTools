using System.Reflection;
using Microsoft.EntityFrameworkCore;
using PrintingTools.Domain.Entities;

namespace PrintingTools.Infrastructure.Data;

public class PrintingDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<PrintJob> PrintJobs { get; set; }
    public DbSet<Printer> Printers { get; set; }

    public PrintingDbContext(DbContextOptions<PrintingDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Применяем все конфигурации из текущей сборки
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Глобальный фильтр для soft delete
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<PrintJob>().HasQueryFilter(p => !p.IsDeleted);
        modelBuilder.Entity<Printer>().HasQueryFilter(pr => !pr.IsDeleted);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateTimestamps();
        return base.SaveChanges();
    }

    private void UpdateTimestamps()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.Entity is Domain.Common.BaseEntity && 
                        (e.State == EntityState.Added || e.State == EntityState.Modified));

        foreach (var entry in entries)
        {
            var entity = (Domain.Common.BaseEntity)entry.Entity;
            
            if (entry.State == EntityState.Modified)
            {
                entry.Property("UpdatedAt").CurrentValue = DateTime.UtcNow;
            }
        }
    }
}