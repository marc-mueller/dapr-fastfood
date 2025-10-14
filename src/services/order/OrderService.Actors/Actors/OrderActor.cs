using System.Diagnostics;
using Dapr.Actors.Runtime;
using Dapr.Client;
using FastFood.Common;
using FinanceService.Observability;
using OrderService.Models.Actors;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;


namespace OrderPlacement.Actors;

public class OrderActor : Actor, IOrderActor, IRemindable
{
    private const string ReminderLostOrderDuringCreation = "OrderLostDuringCreation";
    private readonly DaprClient _daprClient;
    private readonly IOrderServiceActorObservability _observability;

    public OrderActor(ActorHost host, DaprClient daprClient, IOrderServiceActorObservability observability) : base(host)
    {
        _daprClient = daprClient;
        _observability = observability;
    }

    public async Task<Order> CreateOrder(Order order)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        Logger.LogInformation($"New order received: {order.Id}");
        order.State = OrderState.Creating;
        order.OrderReference = $"O{Random.Shared.Next(1,999)}";
        order.CreatedAt = DateTimeOffset.UtcNow;
        _observability.OrdersCreatedCounter.Add(1, new KeyValuePair<string, object?>("orderType", order.Type));
        await StateManager.SetStateAsync("order", order);
        await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderCreated, order.ToDto());
        await RegisterReminderAsync(ReminderLostOrderDuringCreation, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

        
        return order;
    }

    public async Task<Order> AssignCustomer(Customer customer)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer = customer;

            await StateManager.SetStateAsync("order", order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());

            return order;
        }
        
        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to assign a customer");
        throw new InvalidOperationException("Order is not in the correct state to assign a customer");    
    }

    public async Task<Order> AssignInvoiceAddress(Address address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.InvoiceAddress = address;

            await StateManager.SetStateAsync("order", order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
            
            return order;
        }

        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to assign an invoice address");
        throw new InvalidOperationException("Order is not in the correct state to assign an invoice address");
    }

    public async Task<Order> AssignDeliveryAddress(Address address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.DeliveryAddress = address;

            await StateManager.SetStateAsync("order", order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
            
            return order;
        }

        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to assign a delivery address");
        throw new InvalidOperationException("Order is not in the correct state to assign a delivery address");
    }

    public async Task<Order> AddItem(OrderItem item)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            if (order.Items == null || order.Items.Count == 0)
            {
                order.Items = new List<OrderItem>();
            }
            
            var existingItem = order.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                order.Items.Add(item);
            }

            await StateManager.SetStateAsync("order", order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());

            
            return order;
        }

        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to add an item");
        throw new InvalidOperationException("Order is not in the correct state to add an item");
    }

    public async Task<Order> RemoveItem(Guid itemId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            var itemToRemove = order.Items?.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null && order.Items != null)
            {
                order.Items.Remove(itemToRemove);
            }

            await StateManager.SetStateAsync("order", order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
            
            return order;
        }

        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to remove an item");
        throw new InvalidOperationException("Order is not in the correct state to remove an item");
    }

    public async Task<Order> ConfirmOrder()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            order.State = OrderState.Confirmed;

            await StateManager.SetStateAsync("order", order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderConfirmed, order.ToDto());
            
            return order;
        }

        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to confirm");
        throw new InvalidOperationException("Order is not in the correct state to confirm");
    }

    public async Task<Order> ConfirmPayment()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Confirmed)
        {
            order.State = OrderState.Paid;
            
            order.PaidAt = DateTimeOffset.UtcNow;
            
            _observability.OrdersPaidCounter.Add(1);
            _observability.OrderItemsCount.Record(order.Items?.Count ?? 0);
            _observability.OrderTotalAmount.Record(order.Items?.Select(i => i.ItemPrice * i.Quantity).Sum() ?? 0);
            
            _observability.OrderSalesDuration.Record((order.PaidAt - order.CreatedAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, order.ToDto());
            
            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/newOrder", order.ToFinanceDto());

            await UnregisterReminderAsync(ReminderLostOrderDuringCreation);

            
            return order;
        }

        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to confirm payment");
        throw new InvalidOperationException("Order is not in the correct state to confirm payment");
    }

    public async Task<Order> StartProcessing()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Paid)
        {
            order.State = OrderState.Processing;
            order.StartProcessingAt = DateTimeOffset.UtcNow;

            await StateManager.SetStateAsync("order", order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated, order.ToDto());
            
            return order;
        }

        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to start processing");
        throw new InvalidOperationException("Order is not in the correct state to start processing");
    }

    // ...
    
    public async Task<Order> FinishedItem(Guid itemId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Processing)
        {
            var itemToUpdate = order.Items?.FirstOrDefault(i => i.Id == itemId);
            if (itemToUpdate != null)
            {
                itemToUpdate.State = OrderItemState.Finished;
                
                if(order.Items != null && order.Items.All(i => i.State == OrderItemState.Finished))
                {
                    order.State = OrderState.Prepared;
                    order.PreparationFinishedAt = DateTimeOffset.UtcNow;
                    _observability.OrderPerparationDuration.Record((order.PreparationFinishedAt - order.StartProcessingAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));
                }

                await StateManager.SetStateAsync("order", order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated, order.ToDto());

                if (order.State == OrderState.Prepared)
                {
                    await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPrepared, order.ToDto());
                }

                
                return order;
            }

            throw new InvalidOperationException("Item not found");
        }

        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to finish an item");
        throw new InvalidOperationException("Order is not in the correct state to remove an item");
    }

    public async Task<Order> Served()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Prepared && order.Type == OrderType.Inhouse)
        {
            order.State = OrderState.Closed;
            order.ClosedAt = DateTimeOffset.UtcNow;
            _observability.OrdersClosedCounter.Add(1, new KeyValuePair<string, object?>("orderId", order.Id));
            _observability.OrderTotalDuration.Record((order.ClosedAt - order.CreatedAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed , order.ToDto());
            
            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/closeOrder", order.Id);
            
            return order;
        }
        
        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to be served");
        throw new InvalidOperationException("Order is not in the correct state to be served");
    }

    public async Task<Order> StartDelivery()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Prepared && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Delivering;
            order.StartDeliveringAt = DateTimeOffset.UtcNow;

            await StateManager.SetStateAsync("order", order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated, order.ToDto());

            
            return order;
        }
        
        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to start delivery");
        throw new InvalidOperationException("Order is not in the correct state to start delivery");
    }

    public async Task<Order> Delivered()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Delivering && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Closed;
            order.ClosedAt = DateTimeOffset.UtcNow;
            order.DeliveredAt = DateTimeOffset.UtcNow;
            
            _observability.OrdersClosedCounter.Add(1, new KeyValuePair<string, object?>("orderId", order.Id));
            _observability.OrderDeliveryDuration.Record((order.DeliveredAt - order.StartDeliveringAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));
            _observability.OrderTotalDuration.Record((order.ClosedAt - order.CreatedAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed, order.ToDto());

            
            return order;
        }
        
        activity?.SetStatus(ActivityStatusCode.Error, "Order is not in the correct state to start delivery");
        throw new InvalidOperationException("Order is not in the correct state to start delivery");
    }

    public async Task<Order> GetOrder()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await StateManager.GetStateAsync<Order>("order");
        return order;
    }

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        if (reminderName == ReminderLostOrderDuringCreation)
        {
            await UnregisterReminderAsync(ReminderLostOrderDuringCreation);
            var order = await StateManager.GetStateAsync<Order>("order");
            Logger.LogInformation($"Lost order during creation {order.Id}");
            // todo: do something regarding the lost order.
        }
    }
}