using Dapr.Client;
using FastFood.Common;
using FastFood.FeatureManagement.Common.Constants;
using FastFood.FeatureManagement.Common.Services;
using Microsoft.FeatureManagement.FeatureFilters;

namespace OrderPlacement.Services;

public class OrderEventRouter : IOrderEventRouter
{
    private readonly DaprClient _daprClient;
    private readonly IObservableFeatureManager _featureManager;

    public OrderEventRouter(DaprClient daprClient, IObservableFeatureManager featureManager)
    {
        _daprClient = daprClient;
        _featureManager = featureManager;
    }
    
    public async Task RegisterOrderForService(Guid orderid, OrderEventRoutingTarget target)
    {
        // Create targeting context for per-order feature evaluation
        // This enables percentage rollouts (e.g., 50% of orders to Workflow, 50% to Actor/State)
        var targetingContext = new TargetingContext
        {
            UserId = orderid.ToString(),
            Groups = new List<string>()
        };
        
        // Check if UseWorkflowImplementation feature is enabled for this specific order
        var useWorkflow = await _featureManager.IsEnabledAsync(
            FeatureFlags.UseWorkflowImplementation, 
            targetingContext
        );
        
        // Override target to Workflow if feature is enabled
        var finalTarget = useWorkflow ? OrderEventRoutingTarget.OrderProcessingServiceWorkflow : target;
        
        await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName, GetStateId(orderid), finalTarget);
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