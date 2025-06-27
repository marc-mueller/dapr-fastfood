using Dapr.Actors;
using Dapr.Actors.Client;
using FinanceService.Observability;
using OrderService.Models.Actors;
using OrderService.Models.Entities;

namespace OrderPlacement.Services;

public class OrderProcessingServiceActor : IOrderProcessingServiceActor
{
    private readonly IOrderEventRouter _orderEventRouter;
    private readonly IOrderServiceObservability _observability;
    private const string OrderActorName = "OrderActor";
    public OrderProcessingServiceActor(IOrderEventRouter orderEventRouter, IOrderServiceObservability observability)
    {
        _orderEventRouter = orderEventRouter;
        _observability = observability;
    }
    
    public async Task CreateOrder(Order order)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(order.Id.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.CreateOrder(order);
        _orderEventRouter.RegisterOrderForService(order.Id, OrderEventRoutingTarget.OrderProcessingServiceActor);
    }

    public async Task AssignCustomer(Guid orderid, Customer customer)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.AssignCustomer(customer);
    }

    public async Task RemoveItem(Guid orderid, Guid itemId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.RemoveItem(itemId);
    }

    public  async Task ConfirmOrder(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.ConfirmOrder();
    }

    public async Task ConfirmPayment(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.ConfirmPayment();
    }

    public async Task StartProcessing(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.StartProcessing();
    }

    public async Task FinishedItem(Guid orderid, Guid itemId)
    {
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.FinishedItem(itemId);
    }

    public async Task Served(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.Served();
        await _orderEventRouter.RemoveRoutingTargetForOrder(orderid);
    }

    public  async Task StartDelivery(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.StartDelivery();
    }

    public async Task Delivered(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.Delivered();
        await _orderEventRouter.RemoveRoutingTargetForOrder(orderid);
    }

    public async Task<Order> GetOrder(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.GetOrder();
        return orderResult;
    }

    public async Task AddItem(Guid orderid, OrderItem item)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.AddItem(item);
    }

    public async Task AssignDeliveryAddress(Guid orderid, Address address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.AssignDeliveryAddress(address);
    }

    public async Task AssignInvoiceAddress(Guid orderid, Address address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var actorId = new ActorId(orderid.ToString());
        var proxy = ActorProxy.Create<IOrderActor>(actorId, OrderActorName);
        var orderResult = await proxy.AssignInvoiceAddress(address);
    }
}