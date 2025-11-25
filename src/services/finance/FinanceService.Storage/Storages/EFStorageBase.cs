using Microsoft.EntityFrameworkCore;

namespace FinanceService.Storage.Storages;

public abstract class EFStorageBase : DbContext
{
    protected EFStorageBase(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EFStorageBase).Assembly);
    }
}