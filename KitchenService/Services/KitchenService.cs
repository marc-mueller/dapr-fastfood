using Dapr.Client;
using FastFood.Common;
using KitchenService.Common.Events;
using KitchenService.Entities;

namespace KitchenService.Services;

public class KitchenService : IKitchenService
{
    private readonly DaprClient _daprClient;
    private readonly Dictionary<Guid, KitchenOrder> _mockStorage;

    public KitchenService(DaprClient daprClient)
    {
        _daprClient = daprClient;
        _mockStorage = new Dictionary<Guid, KitchenOrder>();
    }

    public Task<KitchenOrder> AddOrder(Guid orderId, string orderReference, IEnumerable<Tuple<Guid, Guid, string, int, string?>> items)
    {
        var order = new KitchenOrder() {Id = orderId, OrderReference = orderReference};
        foreach (var item in items)
        {
            order.Items.Add(new KitchenOrderItem() { OrderId = order.Id, Id = item.Item1, ProductId = item.Item2, ProductDescription  = item.Item3, Quantity = item.Item4, CustomerComments = item.Item5});
        }
        _mockStorage.Add(order.Id, order);
        
        _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.KitchenOrderStartProcessing, new KitchenOrderStartProcessingEvent(){ OrderId = order.Id });
        return Task.FromResult(order);
    }
    
    public Task<IEnumerable<KitchenOrder>> GetPendingOrders()
    {
        return Task.FromResult(_mockStorage.Values.Where(o => o.Items.Any(i => i.State == KitchenOrderItemState.AwaitingPreparation)));
    }
    
    public Task<KitchenOrder?> GetPendingOrder(Guid id)
    {
        return Task.FromResult(_mockStorage.Values.Where(o => o.Items.Any(i => i.State == KitchenOrderItemState.AwaitingPreparation)).SingleOrDefault(o => o.Id == id));
    }
    
    public Task<IEnumerable<KitchenOrderItem>> GetPendingItems()
    {
        return Task.FromResult(_mockStorage.Values.SelectMany(o => o.Items).Where(i => i.State == KitchenOrderItemState.AwaitingPreparation));
    }

    public Task<KitchenOrderItem> SetItemAsFinished(Guid id)
    {
        var item = _mockStorage.Values.SelectMany(o => o.Items).FirstOrDefault(i => i.Id == id);
        if (item != null)
        {
            item.State = KitchenOrderItemState.Finished;
            _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, "kitchenitemfinished", new KitchenItemFinishedEvent(){ OrderId = item.OrderId, ItemId = item.Id });
            
            var order = _mockStorage[item.OrderId];
            if (order.Items.All(i => i.State == KitchenOrderItemState.Finished))
            {
                _mockStorage.Remove(order.Id);
            }
            
            return Task.FromResult(item);
        }
        
        throw new InvalidOperationException("Item not found");
    }
    
}