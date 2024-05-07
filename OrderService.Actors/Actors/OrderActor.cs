using Dapr.Actors.Runtime;
using Dapr.Client;
using FastFood.Common;
using OrderService.Models.Actors;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;


namespace OrderPlacement.Actors;

public class OrderActor : Actor, IOrderActor, IRemindable
{
    private const string ReminderLostOrderDuringCreation = "OrderLostDuringCreation";
    private readonly DaprClient _daprClient;

    public OrderActor(ActorHost host, DaprClient daprClient) : base(host)
    {
        _daprClient = daprClient;
    }

    public async Task<Order> CreateOrder(Order order)
    {
        Logger.LogInformation($"New order received: {order.Id}");
        order.State = OrderState.Creating;
        await StateManager.SetStateAsync("order", order);

        await RegisterReminderAsync(ReminderLostOrderDuringCreation, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

        
        return order;
    }

    public async Task<Order> AssignCustomer(Customer customer)
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer = customer;

            await StateManager.SetStateAsync("order", order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign a customer");
    }

    public async Task<Order> AssignInvoiceAddress(Address address)
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.InvoiceAddress = address;

            await StateManager.SetStateAsync("order", order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign an invoice address");
    }

    public async Task<Order> AssignDeliveryAddress(Address address)
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.DeliveryAddress = address;

            await StateManager.SetStateAsync("order", order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign a delivery address");
    }

    public async Task<Order> AddItem(OrderItem item)
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            if (order.Items == null || order.Items.Count == 0)
            {
                order.Items = new List<OrderItem>();
            }
            order.Items?.Add(item);

            await StateManager.SetStateAsync("order", order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to add an item");
    }

    public async Task<Order> RemoveItem(Guid itemId)
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            var itemToRemove = order.Items?.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null && order.Items != null)
            {
                order.Items.Remove(itemToRemove);
            }

            await StateManager.SetStateAsync("order", order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to remove an item");
    }

    public async Task<Order> ConfirmOrder()
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Creating)
        {
            order.State = OrderState.Confirmed;

            await StateManager.SetStateAsync("order", order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to confirm");
    }

    public async Task<Order> ConfirmPayment()
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Confirmed)
        {
            order.State = OrderState.Paid;

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, order.ToDto());
            
            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/newOrder", order.ToFinanceDto());

            await UnregisterReminderAsync(ReminderLostOrderDuringCreation);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to confirm payment");
    }

    public async Task<Order> StartProcessing()
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Paid)
        {
            order.State = OrderState.Processing;

            await StateManager.SetStateAsync("order", order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to start processing");
    }

    public async Task<Order> FinishedItem(Guid itemId)
    {
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
                }

                await StateManager.SetStateAsync("order", order);

                if (order.State == OrderState.Prepared)
                {
                    await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPrepared, order.ToDto());
                }

                
                return order;
            }

            throw new InvalidOperationException("Item not found");
        }

        throw new InvalidOperationException("Order is not in the correct state to remove an item");
    }

    public async Task<Order> Served()
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Prepared && order.Type == OrderType.Inhouse)
        {
            order.State = OrderState.Closed;

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed , order.ToDto());
            
            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/closeOrder", order.Id);
            
            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to be served");
    }

    public async Task<Order> StartDelivery()
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Prepared && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Delivering;

            await StateManager.SetStateAsync("order", order);

            
            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to start delivery");
    }

    public async Task<Order> Delivered()
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        if (order.State == OrderState.Delivering && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Closed;

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed, order.ToDto());

            
            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to start delivery");
    }

    public async Task<Order> GetOrder()
    {
        var order = await StateManager.GetStateAsync<Order>("order");
        return order;
    }

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName == ReminderLostOrderDuringCreation)
        {
            await UnregisterReminderAsync(ReminderLostOrderDuringCreation);
            var order = await StateManager.GetStateAsync<Order>("order");
            Logger.LogInformation($"Lost order during creation {order.Id}");
            // todo: do something regarding the lost order.
        }

        
    }
}