using System.Diagnostics;
using Dapr.Client;
using FastFood.Common;
using FastFood.FeatureManagement.Common.Constants;
using FastFood.FeatureManagement.Common.Services;
using FastFood.FeatureManagement.Common.Telemetry;
using FinanceService.Observability;
using KitchenService.Common.Events;
using KitchenService.Entities;

namespace KitchenService.Services;

public class KitchenService : IKitchenService
{
    private readonly DaprClient _daprClient;
    private readonly Dictionary<Guid, KitchenOrder> _mockStorage;
    private readonly IKitchenServiceObservability _observability;
    private readonly IObservableFeatureManager _featureManager;

    public KitchenService(DaprClient daprClient, IKitchenServiceObservability observability, IObservableFeatureManager featureManager)
    {
        _daprClient = daprClient;
        _observability = observability;
        _featureManager = featureManager;
        _mockStorage = new Dictionary<Guid, KitchenOrder>();
    }

    public Task<KitchenOrder> AddOrder(Guid orderId, string orderReference, IEnumerable<Tuple<Guid, Guid, string, int, string?>> items)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = new KitchenOrder() {Id = orderId, OrderReference = orderReference};
        order.CreatedAt = DateTimeOffset.UtcNow;
        foreach (var item in items)
        {
            order.Items.Add(new KitchenOrderItem() { OrderId = order.Id, Id = item.Item1, ProductId = item.Item2, ProductDescription  = item.Item3, Quantity = item.Item4, CustomerComments = item.Item5, State = KitchenOrderItemState.AwaitingPreparation, CreatedAt = DateTimeOffset.UtcNow });
        }
        _mockStorage.Add(order.Id, order);
        
        _observability.OrdersCounter.Add(1);
        _observability.OrderItemsCount.Record(order.Items.Count);
        
        _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.KitchenOrderStartProcessing, new KitchenOrderStartProcessingEvent(){ OrderId = order.Id });
        return Task.FromResult(order);
    }
    
    public Task<IEnumerable<KitchenOrder>> GetPendingOrders()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        return Task.FromResult(_mockStorage.Values.Where(o => o.Items.Any(i => i.State == KitchenOrderItemState.AwaitingPreparation)));
    }
    
    public Task<KitchenOrder?> GetPendingOrder(Guid id)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        return Task.FromResult(_mockStorage.Values.Where(o => o.Items.Any(i => i.State == KitchenOrderItemState.AwaitingPreparation)).SingleOrDefault(o => o.Id == id));
    }
    
    public async Task<IEnumerable<KitchenOrderItem>> GetPendingItems()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        
        var pendingItems = _mockStorage.Values.SelectMany(o => o.Items)
            .Where(i => i.State == KitchenOrderItemState.AwaitingPreparation);
        
        // Check if auto-prioritization feature is enabled
        var autoPrioritizationEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.AutoPrioritization);
        
        if (autoPrioritizationEnabled)
        {
            // Calculate priority based on wait time and estimated prep time
            var prioritizedItems = pendingItems
                .Select(item => new
                {
                    Item = item,
                    WaitTime = (DateTimeOffset.UtcNow - item.CreatedAt).TotalMinutes,
                    EstimatedPrepTime = EstimatePreparationTime(item.ProductDescription),
                    Priority = CalculatePriority(item)
                })
                .OrderByDescending(x => x.Priority)
                .ThenBy(x => x.EstimatedPrepTime)
                .Select(x => x.Item)
                .ToList();
            
            // Record feature usage
            var reorderedCount = prioritizedItems.Count;
            FeatureFlagActivityEnricher.RecordFeatureUsage(activity, FeatureFlags.AutoPrioritization, "queue_reordered");
            FeatureFlagActivityEnricher.RecordFeatureMetric(activity, FeatureFlags.AutoPrioritization, "items_reordered", reorderedCount);
            
            _observability.FeatureUsageCounter.Add(1,
                new KeyValuePair<string, object?>("feature", FeatureFlags.AutoPrioritization),
                new KeyValuePair<string, object?>("action", "queue_reordered"),
                new KeyValuePair<string, object?>("items_count", reorderedCount));
            
            return prioritizedItems;
        }
        
        // Default: return in creation order
        return pendingItems.OrderBy(i => i.CreatedAt);
    }
    
    private double CalculatePriority(KitchenOrderItem item)
    {
        var waitTime = (DateTimeOffset.UtcNow - item.CreatedAt).TotalMinutes;
        var estimatedPrepTime = EstimatePreparationTime(item.ProductDescription);
        
        // Priority = wait time / estimated prep time
        // Items that have been waiting longer relative to their prep time get higher priority
        return estimatedPrepTime > 0 ? waitTime / estimatedPrepTime : waitTime;
    }
    
    private double EstimatePreparationTime(string? productDescription)
    {
        if (string.IsNullOrEmpty(productDescription))
            return 5.0; // Default 5 minutes
        
        var lowerDesc = productDescription.ToLower();
        
        // Simple heuristics for demo purposes
        if (lowerDesc.Contains("burger") || lowerDesc.Contains("sandwich"))
            return 8.0;
        if (lowerDesc.Contains("fries") || lowerDesc.Contains("drink"))
            return 3.0;
        if (lowerDesc.Contains("dessert") || lowerDesc.Contains("ice cream"))
            return 4.0;
        if (lowerDesc.Contains("salad"))
            return 6.0;
        
        return 5.0; // Default
    }

    public Task<KitchenOrderItem> SetItemAsFinished(Guid id)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var item = _mockStorage.Values.SelectMany(o => o.Items).FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            item.State = KitchenOrderItemState.Finished;
            item.FinishedAt = DateTimeOffset.UtcNow;
            _observability.OrderItemPreparationDuration.Record((item.FinishedAt - item.CreatedAt).TotalSeconds);
            
            _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, "kitchenitemfinished", new KitchenItemFinishedEvent(){ OrderId = item.OrderId, ItemId = item.Id });
            
            var order = _mockStorage[item.OrderId];
            if (order.Items.All(i => i.State == KitchenOrderItemState.Finished))
            {
                order.FinishedAt = DateTimeOffset.UtcNow;
                _observability.OrderPerparationDuration.Record((order.FinishedAt - order.CreatedAt).TotalSeconds);
                _mockStorage.Remove(order.Id);
            }
            
            return Task.FromResult(item);
        }
        activity?.SetStatus(ActivityStatusCode.Error, "Item not found");
        throw new InvalidOperationException("Item not found");
    }
    
}