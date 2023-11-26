using Dapr.Actors;
using Dapr.Actors.Client;
using OrderService.Models.Actors;
using OrderService.Models.Entities;

namespace OrderPlacement.Services;

public class OrderProcessingServiceActor : IOrderProcessingService
{
    private const string OrderActorName = "OrderActor";
    
    // private readonly IReadStorage _readStorage;

    // public OrderProcessingServiceActor(IReadStorage readStorage)
    public OrderProcessingServiceActor()
    {
            // _readStorage = readStorage;
    }

    // public Task<Order> GetOrder(Guid id)
    // {
    //     return  _readStorage.GetOrderById(id);
    // }
    //
    // public Task<IEnumerable<Order>> GetOrders()
    // {
    //     return  _readStorage.GetOrders();
    // }
    // public Task<IEnumerable<Order>> GetActiveOrders()
    // {
    //     return  _readStorage.GetActiveOrders();
    // }
    
    
    public async Task<Order> CreateOrder(Order order)
    {
        var actorId = new ActorId(order.Id.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.CreateOrder(order);
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> AssignCustomer(Guid orderid, Customer customer)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.AssignCustomer(customer);
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> RemoveItem(Guid orderid, Guid itemId)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.RemoveItem(itemId);
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public  async Task<Order> ConfirmOrder(Guid orderid)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.ConfirmOrder();
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> ConfirmPayment(Guid orderid)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.ConfirmPayment();
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> StartProcessing(Guid orderid)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.StartProcessing();
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> FinishedItem(Guid orderid, Guid itemId)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.FinishedItem(itemId);
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> Served(Guid orderid)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.Served();
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public  async Task<Order> StartDelivery(Guid orderid)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.StartDelivery();
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> Delivered(Guid orderid)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.Delivered();
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> GetOrder(Guid orderid)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.GetOrder();
        return orderResult;
    }

    public async Task<Order> AddItem(Guid orderid, OrderItem item)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.AddItem(item);
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> AssignDeliveryAddress(Guid orderid, Address address)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.AssignDeliveryAddress(address);
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }

    public async Task<Order> AssignInvoiceAddress(Guid orderid, Address address)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.AssignInvoiceAddress(address);
        //await _readStorage.UpdateOder(orderResult);
        return orderResult;
    }
}