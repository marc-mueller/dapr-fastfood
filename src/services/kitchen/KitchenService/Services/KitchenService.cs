using System.Diagnostics;
using Dapr.Client;
using FastFood.Common;
using FinanceService.Observability;
using KitchenService.Common.Events;
using KitchenService.Entities;

namespace KitchenService.Services;

public class KitchenService : IKitchenService
{
    private readonly DaprClient _daprClient;
    private readonly Dictionary<Guid, KitchenOrder> _mockStorage;
    private readonly IKitchenServiceObservability _observability;

    public KitchenService(DaprClient daprClient, IKitchenServiceObservability observability)
    {
        _daprClient = daprClient;
        _observability = observability;
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
    
    public Task<IEnumerable<KitchenOrderItem>> GetPendingItems()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        return Task.FromResult(_mockStorage.Values.SelectMany(o => o.Items).Where(i => i.State == KitchenOrderItemState.AwaitingPreparation));
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