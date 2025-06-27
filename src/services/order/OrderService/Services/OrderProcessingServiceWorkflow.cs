using Dapr.Workflow;
using FinanceService.Observability;
using OrderPlacement.Storages;
using OrderPlacement.Workflows;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;

namespace OrderPlacement.Services;

public class OrderProcessingServiceWorkflow : IOrderProcessingServiceWorkflow
{
    private readonly DaprWorkflowClient _daprWorkflowClient;
    private readonly ILogger<OrderProcessingServiceWorkflow> _logger;
    private readonly IOrderStorage _orderStorage;
    private readonly IOrderEventRouter _orderEventRouter;
    private readonly IOrderServiceObservability _observability;

    public OrderProcessingServiceWorkflow(DaprWorkflowClient daprWorkflowClient, IOrderStorage orderStorage, IOrderEventRouter orderEventRouter, IOrderServiceObservability observability, ILogger<OrderProcessingServiceWorkflow> logger)
    {
        _daprWorkflowClient = daprWorkflowClient;
        _orderStorage = orderStorage;
        _orderEventRouter = orderEventRouter;
        _observability = observability;
        _logger = logger;
    }
    public async Task<Order> GetOrder(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        return await _orderStorage.GetOrderById(orderid);
    }

    public async Task CreateOrder(Order order)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        await _daprWorkflowClient.ScheduleNewWorkflowAsync(nameof(OrderProcessingWorkflow), order.Id.ToString(), order.Id);
        await _orderEventRouter.RegisterOrderForService(order.Id, OrderEventRoutingTarget.OrderProcessingServiceWorkflow);
    }

    public async Task AssignCustomer(Guid orderid, Customer customer)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        // await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), AssignCustomerEvent.Name, new AssignCustomerEvent(){ OrderId = orderid, Customer = customer });
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), CreateOrderWrapperEvent.Name, new CreateOrderWrapperEvent(new AssignCustomerEvent(){ OrderId = orderid, Customer = customer }));
    }

    public async Task AssignInvoiceAddress(Guid orderid, Address address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        // await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), AssignInvoiceAddressEvent.Name, new AssignInvoiceAddressEvent(){ OrderId = orderid, Address = address });
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), CreateOrderWrapperEvent.Name, new CreateOrderWrapperEvent( new AssignInvoiceAddressEvent(){ OrderId = orderid, Address = address }));
    }

    public async Task AssignDeliveryAddress(Guid orderid, Address address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        // await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), AssignDeliveryAddressEvent.Name, new AssignDeliveryAddressEvent(){ OrderId = orderid, Address = address });
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), CreateOrderWrapperEvent.Name, new CreateOrderWrapperEvent( new AssignDeliveryAddressEvent(){ OrderId = orderid, Address = address }));
    }

    public async Task AddItem(Guid orderid, OrderItem item)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        // await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), AddItemEvent.Name, new AddItemEvent(){ OrderId = orderid, Item = item });
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), CreateOrderWrapperEvent.Name, new CreateOrderWrapperEvent( new AddItemEvent(){ OrderId = orderid, Item = item }));
    }

    public async Task RemoveItem(Guid orderid, Guid itemId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        // await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), RemoveItemEvent.Name, new RemoveItemEvent(){ OrderId = orderid, ItemId = itemId });
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), CreateOrderWrapperEvent.Name, new CreateOrderWrapperEvent( new RemoveItemEvent(){ OrderId = orderid, ItemId = itemId }));
    }

    public async Task ConfirmOrder(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        // await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), ConfirmOrderEvent.Name, new ConfirmOrderEvent(){ OrderId = orderid });
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), CreateOrderWrapperEvent.Name, new CreateOrderWrapperEvent( new ConfirmOrderEvent(){ OrderId = orderid }));
    }

    public async Task ConfirmPayment(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), ConfirmPaymentEvent.Name, new ConfirmPaymentEvent(){ OrderId = orderid });
    }

    public async Task StartProcessing(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), StartProcessingEvent.Name, new StartProcessingEvent(){ OrderId = orderid });
    }

    public async Task FinishedItem(Guid orderid, Guid itemId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), ItemFinishedEvent.Name, new ItemFinishedEvent(){ OrderId = orderid, ItemId = itemId });
    }

    public async Task Served(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), OrderServedEvent.Name, new OrderServedEvent(){ OrderId = orderid });
        await _orderEventRouter.RemoveRoutingTargetForOrder(orderid);
    }

    public async Task StartDelivery(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), StartDeliveryEvent.Name, new StartDeliveryEvent(){ OrderId = orderid });
    }

    public async Task Delivered(Guid orderid)
    {
        await _daprWorkflowClient.RaiseEventAsync(orderid.ToString(), DeliveredEvent.Name, new DeliveredEvent(){ OrderId = orderid });
        await _orderEventRouter.RemoveRoutingTargetForOrder(orderid);
    }
}