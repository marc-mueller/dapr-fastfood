using Dapr.Client;
using FastFood.Common;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Services;

public class OrderProcessingServiceState : IOrderProcessingService
{
    private readonly DaprClient _daprClient;

    public OrderProcessingServiceState(DaprClient daprClient)
    {
        _daprClient = daprClient;
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

    

    // public Task<IEnumerable<Order>> GetOrders()
    // {
    //     throw new NotImplementedException();
    // }
    //
    // public Task<IEnumerable<Order>> GetActiveOrders()
    // {
    //     throw new NotImplementedException();
    // }

    public async Task<Order> CreateOrder(Order order)
    {
        order.State = OrderState.Creating;
        await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
        return order;
    }

    public async Task<Order> AssignCustomer(Guid orderid, Customer customer)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.Customer = customer;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign a customer");
    }

    public async Task<Order> AssignInvoiceAddress(Guid orderid, Address address)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.InvoiceAddress = address;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign an invoice address");
    }

    public async Task<Order> AssignDeliveryAddress(Guid orderid, Address address)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.DeliveryAddress = address;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to assign a delivery address");
    }

    public async Task<Order> AddItem(Guid orderid, OrderItem item)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.Items?.Add(item);

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to add an item");
    }

    public async Task<Order> RemoveItem(Guid orderid, Guid itemId)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            var itemToRemove = order.Items?.FirstOrDefault(i => i.Id == itemId);
            if (itemToRemove != null)
            {
                order.Items.Remove(itemToRemove);
            }

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to remove an item");
    }

    public async Task<Order> ConfirmOrder(Guid orderid)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Creating)
        {
            order.State = OrderState.Confirmed;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to confirm");
    }

    public async Task<Order> ConfirmPayment(Guid orderid)
    {
        var order = await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Confirmed)
        {
            order.State = OrderState.Paid;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, order.ToDto());

            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/newOrder", order.ToFinanceDto());

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to confirm payment");
    }

    public async Task<Order> StartProcessing(Guid orderid)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Paid)
        {
            order.State = OrderState.Processing;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

            
            return order;
        }

        throw new InvalidOperationException("Order is not in the correct state to start processing");
    }

    public async Task<Order> FinishedItem(Guid orderid, Guid itemId)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Processing)
        {
            var itemToUpdate = order.Items?.FirstOrDefault(i => i.Id == itemId);
            if (itemToUpdate != null)
            {
                itemToUpdate.State = OrderItemState.Finished;
                
                if(order.Items.All(i => i.State == OrderItemState.Finished))
                {
                    order.State = OrderState.Prepared;
                }

                await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

                if (order.State == OrderState.Prepared)
                {
                    await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPrepared, order.ToDto());
                }

                
                return order;
            }

            throw new InvalidOperationException("Item not found");
        }

        throw new InvalidOperationException("Order is not in the correct state to finish an item");
    }

    public async Task<Order> Served(Guid orderid)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Prepared && order.Type == OrderType.Inhouse)
        {
            order.State = OrderState.Closed;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed, order.ToDto());
            
            await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/closeOrder", order.Id);

            
            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to be served");
    }

    public async Task<Order> StartDelivery(Guid orderid)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Prepared && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Delivering;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);

            
            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to start delivery");
    }

    public async Task<Order> Delivered(Guid orderid)
    {
        var order =  await _daprClient.GetStateAsync<Order>(FastFoodConstants.StateStoreName, GetStateId(orderid));
        if (order.State == OrderState.Delivering && order.Type == OrderType.Delivery)
        {
            order.State = OrderState.Closed;

            await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName,GetStateId(order.Id), order);
            
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed, order.ToDto());

            
            return order;
        }
        throw new InvalidOperationException("Order is not in the correct state to start delivery");
    }
}