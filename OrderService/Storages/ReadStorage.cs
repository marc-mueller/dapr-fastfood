using OrderService.Models.Entities;

namespace OrderPlacement.Storages;

public class ReadStorage : IReadStorage
{
    private readonly Dictionary<Guid, Order> _mockStorage;

    public ReadStorage()
    {
        _mockStorage = new Dictionary<Guid, Order>();
    }
    
    public Task<IEnumerable<Order>> GetOrders()
    {
        return Task.FromResult(_mockStorage.Values.AsEnumerable());        
    }

    public Task<Order> GetOrderById(Guid id)
    {
        return Task.FromResult(_mockStorage[id]);
    }

    public Task<IEnumerable<Order>> GetActiveOrders()
    {
        return Task.FromResult(_mockStorage.Values.Where(o => o.State !=  OrderState.Closed).AsEnumerable());
    }

    public Task<Order> UpdateOder(Order order)
    {
        _mockStorage[order.Id] = order;
        return Task.FromResult(order);
    }
}