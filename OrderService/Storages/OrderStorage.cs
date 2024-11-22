using Dapr.Client;
using FastFood.Common;
using OrderService.Models.Entities;

namespace OrderPlacement.Storages;

public class OrderStorage : IOrderStorage
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<OrderStorage> _logger;


    public OrderStorage(DaprClient daprClient, ILogger<OrderStorage> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }
    
    public async Task<IEnumerable<Order>> GetOrders()
    {
        throw new NotImplementedException();        
    }

    public async Task<Order> GetOrderById(Guid id)
    {
        return await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(id));
    }

    public Task<IEnumerable<Order>> GetActiveOrders()
    {
        throw new NotImplementedException();
    }

    public async Task<Order> UpdateOder(Order order)
    {
        await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName, GetStateId(order.Id), order);
        return order;
    }
    
    private string GetStateId(Guid orderid)
    {
        return $"OrderStore-{orderid}";
    }
}