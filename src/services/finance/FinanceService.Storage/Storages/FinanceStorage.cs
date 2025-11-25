using FinanceService.Entities;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Storage.Storages;

public class FinanceStorage : EFStorageBase
{
    public FinanceStorage(DbContextOptions<FinanceStorage> options) : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
}