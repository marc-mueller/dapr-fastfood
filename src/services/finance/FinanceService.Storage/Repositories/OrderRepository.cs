using FinanceService.Entities;
using FinanceService.Storage.Storages;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Storage.Repositories;

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetOrdersByStateAsync(OrderState state, CancellationToken cancellationToken = default);
    Task<IEnumerable<Order>> GetOrdersCreatedBetweenAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

public class OrderRepository : RepositoryBase<Order>, IOrderRepository
{
    public OrderRepository(FinanceStorage context) : base(context)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByStateAsync(OrderState state, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.State == state)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetOrdersCreatedBetweenAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
