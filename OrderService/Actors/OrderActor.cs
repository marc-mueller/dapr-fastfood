using Dapr.Actors.Runtime;
using Dapr.Client;
using OrderService.Common.Dtos;

namespace OrderPlacement.Actors;

public class OrderActor : Actor, IOrderActor, IRemindable
{
    private const string ReminderLostOrderDuringCreation = "OrderLostDuringCreation";
    private readonly DaprClient _daprClient;

    public OrderActor(ActorHost host, DaprClient daprClient) : base(host)
    {
        _daprClient = daprClient;
    }

    public async Task<OrderDto> CreateOrder(OrderDto order)
    {
        Logger.LogInformation($"New order received: {order.Id}");
        order.State = OrderState.Creating;
        await StateManager.SetStateAsync("order", order);

        await RegisterReminderAsync(ReminderLostOrderDuringCreation, null, TimeSpan.FromMinutes(30), TimeSpan.FromMinutes(30));

        return order;
    }

    public async Task<OrderDto> AssignCustomer(CustomerDto customer)
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer = customer;

            await StateManager.SetStateAsync("order", order);

            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign a customer");
    }

    public async Task<OrderDto> AssignInvoiceAddress(Address address)
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new CustomerDto();
            order.Customer.InvoiceAddress = address;

            await StateManager.SetStateAsync("order", order);

            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign an invoice address");
    }

    public async Task<OrderDto> AssignDeliveryAddress(Address address)
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new CustomerDto();
            order.Customer.DeliveryAddress = address;

            await StateManager.SetStateAsync("order", order);

            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign a delivery address");
    }

    public async Task<OrderDto> AddItem(OrderItem item)
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Creating)
        {
            order.Items.Add(item);

            await StateManager.SetStateAsync("order", order);

            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to add an item");
    }

    public async Task<OrderDto> RemoveItem(Guid itemId)
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Creating)
        {
            var itemToRemove = order.Items.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null)
            {
                order.Items.Remove(itemToRemove);
            }

            await StateManager.SetStateAsync("order", order);

            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to remove an item");
    }

    public async Task<OrderDto> ConfirmOrder()
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Creating)
        {
            order.State = OrderState.Confirmed;

            await StateManager.SetStateAsync("order", order);

            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to confirm");
    }

    public async Task<OrderDto> ConfirmPayment()
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Confirmed)
        {
            order.State = OrderState.Paid;

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync("pubsub", "orderpaid", order);

            await UnregisterReminderAsync(ReminderLostOrderDuringCreation);

            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to confirm payment");
    }

    public async Task<OrderDto> StartProcessing()
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Paid)
        {
            order.State = OrderState.Processing;

            await StateManager.SetStateAsync("order", order);

            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to start processing");
    }

    public async Task<OrderDto> FinishedItem(Guid itemId)
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Processing)
        {
            var itemToUpdate = order.Items.FirstOrDefault(i => i.Id == itemId);
            if (itemToUpdate != null)
            {
                itemToUpdate.State = OrderItemState.Finished;
                
                if(order.Items.All(i => i.State == OrderItemState.Finished))
                {
                    order.State = OrderState.Prepared;
                }

                await StateManager.SetStateAsync("order", order);

                if (order.State == OrderState.Prepared)
                {
                    await _daprClient.PublishEventAsync("pubsub", "orderprepared", order);
                }

                return order;
            }

            throw new InvalidOperationException("Item not found");
        }

        throw new InvalidOperationException("Order is not in the correct state to remove an item");
    }

    public async Task<OrderDto> Served()
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Prepared && order.Type == OrderType.Inhouse)
        {
            order.State = OrderState.Closed;

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync("pubsub", "orderclosed", order);

            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to be served");
    }

    public async Task<OrderDto> StartDelivery()
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Prepared && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Delivering;

            await StateManager.SetStateAsync("order", order);

            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to start delivery");
    }

    public async Task<OrderDto> Delivered()
    {
        var order = await StateManager.GetStateAsync<OrderDto>("order");
        if (order.State == OrderState.Delivering && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Closed;

            await StateManager.SetStateAsync("order", order);
            
            await _daprClient.PublishEventAsync("pubsub", "orderclosed", order);

            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to start delivery");
    }

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        if (reminderName == ReminderLostOrderDuringCreation)
        {
            await UnregisterReminderAsync(ReminderLostOrderDuringCreation);
            var order = await StateManager.GetStateAsync<OrderDto>("order");
            Logger.LogInformation($"Lost order during creation {order.Id}");
            // todo: do something regarding the lost order.
        }

        
    }
}