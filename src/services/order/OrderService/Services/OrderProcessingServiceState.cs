using Dapr.Client;
using FastFood.Common;
using FastFood.FeatureManagement.Common.Constants;
using FastFood.FeatureManagement.Common.Services;
using FinanceService.Observability;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Services;

public partial class OrderProcessingServiceState : IOrderProcessingServiceState
{
    private readonly DaprClient _daprClient;
    private readonly IOrderEventRouter _orderEventRouter;
    private readonly IOrderServiceObservability _observability;
    private readonly ILogger<OrderProcessingServiceState> _logger;
    private readonly IOrderPricingService _pricingService;
    private readonly IObservableFeatureManager _featureManager;

    public OrderProcessingServiceState(
        DaprClient daprClient, 
        IOrderEventRouter orderEventRouter, 
        IOrderServiceObservability observability, 
        ILogger<OrderProcessingServiceState> logger,
        IOrderPricingService pricingService,
        IObservableFeatureManager featureManager)
    {
        _daprClient = daprClient;
        _orderEventRouter = orderEventRouter;
        _observability = observability;
        _logger = logger;
        _pricingService = pricingService;
        _featureManager = featureManager;
    }
    
    private string GetStateId(Guid orderid)
    {
        return $"OrderProcessing-{orderid}";
    }
 
    public async Task<Order> GetOrder(Guid orderid)
    {
        var order = await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order != null)
        {
            return order;
        }

        throw new InvalidOperationException("Order not found");
    }
    public async Task CreateOrder(Order order)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        order.State = OrderState.Creating;
        order.OrderReference = $"O{Random.Shared.Next(1,999)}";
        order.CreatedAt = DateTimeOffset.UtcNow;
        _observability.OrdersCreatedCounter.Add(1, new KeyValuePair<string, object?>("orderType", order.Type));
        await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
        await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderCreated, order.ToDto());
        await _orderEventRouter.RegisterOrderForService(order.Id, OrderEventRoutingTarget.OrderProcessingServiceState);

        var clientChannel = activity?.GetBaggageItem("clientchannel");
        if (!string.IsNullOrEmpty(clientChannel))
        {
            _observability.ClientChannelOrdersCreatedCounter.Add(1, new KeyValuePair<string, object?>(clientChannel, order.Type));
            LogClientChannelOrdersCreatedCounter(clientChannel, order.Type);
        }
    }

    public async Task AssignCustomer(Guid orderid, Customer customer)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.Customer = customer;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to assign a customer");
        }
    }

    public async Task AssignInvoiceAddress(Guid orderid, Address address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.InvoiceAddress = address;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to assign an invoice address");
        }
    }

    public async Task AssignDeliveryAddress(Guid orderid, Address address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.DeliveryAddress = address;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to assign a delivery address");
        }
    }

    public async Task AddItem(Guid orderid, OrderItem item)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            // Check if an item with the same ProductId already exists
            var existingItem = order.Items?.FirstOrDefault(i => i.ProductId == item.ProductId);
            if (existingItem != null)
            {
                // Update the quantity of the existing item
                existingItem.Quantity += item.Quantity;
            }
            else
            {
                // Add the new item
                order.Items?.Add(item);
            }

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to add an item");
        }
    }

    public async Task RemoveItem(Guid orderid, Guid itemId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            var itemToRemove = order.Items?.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null && order.Items != null)
            {
                order.Items.Remove(itemToRemove);
            }

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to remove an item");
        }
    }

    public async Task ConfirmOrder(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.State = OrderState.Confirmed;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderConfirmed, order.ToDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to confirm");
        }
    }

    public async Task ConfirmPayment(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order = await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Confirmed)
        {
            order.State = OrderState.Paid;
            order.PaidAt = DateTimeOffset.UtcNow;
            
            _observability.OrdersPaidCounter.Add(1);
            _observability.OrderItemsCount.Record(order.Items?.Count ?? 0);
            _observability.OrderTotalAmount.Record(order.Items?.Select(i => i.ItemPrice * i.Quantity).Sum() ?? 0);
            
            _observability.OrderSalesDuration.Record((order.PaidAt - order.CreatedAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, order.ToDto());

            // Calculate pricing breakdown with service fees
            var pricing = await _pricingService.CalculateOrderPricing(order);
            
            // Check for loyalty discount
            decimal? discount = null;
            var loyaltyEnabled = await _featureManager.IsEnabledAsync(FeatureFlags.LoyaltyProgram);
            if (loyaltyEnabled && !string.IsNullOrEmpty(order.Customer?.LoyaltyNumber))
            {
                discount = Math.Round(pricing.Subtotal * 0.10m, 2);
            }
            
            // Send to FinanceService with pricing breakdown
            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/newOrder", order.ToFinanceDto(pricing.ServiceFee, discount));
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to confirm payment");
        }
    }

    public async Task StartProcessing(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Paid)
        {
            order.State = OrderState.Processing;
            order.StartProcessingAt = DateTimeOffset.UtcNow;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated, order.ToDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to start processing");
        }
    }

    public async Task FinishedItem(Guid orderid, Guid itemId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
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

                await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated, order.ToDto());

                if (order.State == OrderState.Prepared)
                {
                    await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPrepared, order.ToDto());
                }
            }
            else
            {
                throw new InvalidOperationException("Item not found");
            }
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to finish an item");    
        }
    }

    public async Task Served(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Prepared && order.Type == OrderType.Inhouse)
        {
            order.State = OrderState.Closed;
            order.ClosedAt = DateTimeOffset.UtcNow;
            _observability.OrdersClosedCounter.Add(1, new KeyValuePair<string, object?>("orderId", order.Id));
            _observability.OrderTotalDuration.Record((order.ClosedAt - order.CreatedAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed, order.ToDto());
            
            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/closeOrder", order.Id);

            await _orderEventRouter.RemoveRoutingTargetForOrder(orderid);
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to be served");
        }
    }

    public async Task StartDelivery(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Prepared && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Delivering;
            order.StartDeliveringAt = DateTimeOffset.UtcNow;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated, order.ToDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to start delivery");
        }
    }

    public async Task Delivered(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Delivering && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Closed;
            order.ClosedAt = DateTimeOffset.UtcNow;
            order.DeliveredAt = DateTimeOffset.UtcNow;
            
            _observability.OrdersClosedCounter.Add(1, new KeyValuePair<string, object?>("orderId", order.Id));
            _observability.OrderDeliveryDuration.Record((order.DeliveredAt - order.StartDeliveringAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));
            _observability.OrderTotalDuration.Record((order.ClosedAt - order.CreatedAt).TotalSeconds, new KeyValuePair<string, object?>("orderType", order.Type));

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed, order.ToDto());

            await _orderEventRouter.RemoveRoutingTargetForOrder(orderid);
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to start delivery");
        }
    }


    [LoggerMessage(LogLevel.Information, "Client channel \"{clientChannel}\" created order of type {orderType}")]
    private partial void LogClientChannelOrdersCreatedCounter(string clientChannel, OrderType orderType);
}