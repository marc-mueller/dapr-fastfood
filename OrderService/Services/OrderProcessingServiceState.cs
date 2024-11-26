using Dapr.Client;
using FastFood.Common;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Services;

public class OrderProcessingServiceState : IOrderProcessingServiceState
{
    private readonly DaprClient _daprClient;
    private readonly IOrderEventRouter _orderEventRouter;

    public OrderProcessingServiceState(DaprClient daprClient, IOrderEventRouter orderEventRouter)
    {
        _daprClient = daprClient;
        _orderEventRouter = orderEventRouter;
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
        order.State = OrderState.Creating;
        order.OrderReference = $"O{Random.Shared.Next(1,999)}";
        await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
        await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderCreated, order.ToDto());
        await _orderEventRouter.RegisterOrderForService(order.Id, OrderEventRoutingTarget.OrderProcessingServiceState);
    }

    public async Task AssignCustomer(Guid orderid, Customer customer)
    {
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
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.Items?.Add(item);

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
        var order = await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Confirmed)
        {
            order.State = OrderState.Paid;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, order.ToDto());

            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/newOrder", order.ToFinanceDto());
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to confirm payment");
        }
    }

    public async Task StartProcessing(Guid orderid)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Paid)
        {
            order.State = OrderState.Processing;

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

        throw new InvalidOperationException("Order is not in the correct state to finish an item");
    }

    public async Task Served(Guid orderid)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Prepared && order.Type == OrderType.Inhouse)
        {
            order.State = OrderState.Closed;

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
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Prepared && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Delivering;

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
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Delivering && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Closed;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed, order.ToDto());

            await _orderEventRouter.RemoveRoutingTargetForOrder(orderid);
        }
        else
        {
            throw new InvalidOperationException("Order is not in the correct state to start delivery");
        }
    }
}