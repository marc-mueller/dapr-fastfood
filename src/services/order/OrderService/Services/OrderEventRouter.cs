using Dapr.Client;
using FastFood.Common;

namespace OrderPlacement.Services;

public class OrderEventRouter : IOrderEventRouter
{
    private readonly DaprClient _daprClient;

    public OrderEventRouter(DaprClient daprClient)
    {
        _daprClient = daprClient;
    }
    
    public async Task RegisterOrderForService(Guid orderid, OrderEventRoutingTarget target)
    {
        await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName, GetStateId(orderid), target);
    } 
    
    public async Task<OrderEventRoutingTarget> GetRoutingTargetForOrder(Guid orderid)
    {
        return await _daprClient.GetStateAsync<OrderEventRoutingTarget>(FastFoodConstants.StateStoreName, GetStateId(orderid));
    }
    
    public async Task RemoveRoutingTargetForOrder(Guid orderid)
    {
        await _daprClient.DeleteStateAsync(FastFoodConstants.StateStoreName, GetStateId(orderid));
    }
    
    private string GetStateId(Guid orderid)
    {
        return $"OrderEventRouterTarget-{orderid}";
    }
}