using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderPlacement.Workflows.Extensions;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

public partial class AssignCustomerActivity : WorkflowActivity<AssignCustomerEvent, Order>
{
    private readonly IOrderStorage _orderStorage;
    private readonly ILogger<AssignCustomerActivity> _logger;
    private readonly DaprClient _daprClient;

    public AssignCustomerActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<AssignCustomerActivity> logger)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, AssignCustomerEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null && order.State == OrderState.Creating)
        {
            order.Customer = input.Customer;
            await _orderStorage.UpdateOrder(order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
            LogAssignedCustomer(context.InstanceId, order.Id, order.Customer.Id);
        }
        else
        {
            LogAssignedCustomerFailed(context.InstanceId, input.OrderId, input.Customer.Id);
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Assigned customer {customerId} to order {orderId}")]
    private partial void LogAssignedCustomer(string instanceId, Guid orderId, Guid customerId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to assign customer {customerId} to order {orderId}")]
    private partial void LogAssignedCustomerFailed(string instanceId, Guid orderId, Guid customerId);
}

public partial class AssignInvoiceAddressActivity : WorkflowActivity<AssignInvoiceAddressEvent, Order>
{
    private readonly IOrderStorage _orderStorage;
    private readonly ILogger<AssignInvoiceAddressActivity> _logger;
    private readonly DaprClient _daprClient;

    public AssignInvoiceAddressActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<AssignInvoiceAddressActivity> logger)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, AssignInvoiceAddressEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null && order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.InvoiceAddress = input.Address;
            await _orderStorage.UpdateOrder(order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
            LogAssignedInvoiceAddress(context.InstanceId, order.Id, order.Customer.InvoiceAddress.ToString());
        }
        else
        {
            LogAssignedInvoiceAddressFailed(context.InstanceId, input.OrderId, input.Address.ToString());
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Assigned invoice address {address} to order {orderId}")]
    private partial void LogAssignedInvoiceAddress(string instanceId, Guid orderId, string address);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to assign invoice address {address} to order {orderId}")]
    private partial void LogAssignedInvoiceAddressFailed(string instanceId, Guid orderId, string address);
}

public partial class AssignDeliveryAddressActivity : WorkflowActivity<AssignDeliveryAddressEvent, Order>
{
    private readonly IOrderStorage _orderStorage;
    private readonly ILogger<AssignDeliveryAddressActivity> _logger;
    private readonly DaprClient _daprClient;

    public AssignDeliveryAddressActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<AssignDeliveryAddressActivity> logger)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, AssignDeliveryAddressEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null && order.State == OrderState.Creating)
        {
            order.Customer ??= new Customer();
            order.Customer.DeliveryAddress = input.Address;
            await _orderStorage.UpdateOrder(order);
            await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
            LogAssignedDeliveryAddress(context.InstanceId, order.Id, order.Customer.DeliveryAddress.ToString());
        }
        else
        {
            LogAssignedDeliveryAddressFailed(context.InstanceId, input.OrderId, input.Address.ToString());
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Assigned delivery address {address} to order {orderId}")]
    private partial void LogAssignedDeliveryAddress(string instanceId, Guid orderId, string address);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to assign delivery address {address} to order {orderId}")]
    private partial void LogAssignedDeliveryAddressFailed(string instanceId, Guid orderId, string address);
}

public partial class AddItemActivity : WorkflowActivity<AddItemEvent, Order>
{
    private readonly IOrderStorage _orderStorage;
    private readonly ILogger<AddItemActivity> _logger;
    private readonly DaprClient _daprClient;

    public AddItemActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<AddItemActivity> logger)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, AddItemEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null && order.State == OrderState.Creating)
        {
            var existingItem = order.Items?.FirstOrDefault(i => i.Id == input.Item.Id);
            if (existingItem != null)
            {
                // item already exists, idempotent operation
                return order;
            }
            else
            {
                order.Items.Add(input.Item);
                await _orderStorage.UpdateOrder(order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
                LogAddedItem(context.InstanceId, order.Id, input.Item.Id);
            }
        }
        else
        {
            LogAddedItemFailed(context.InstanceId, input.OrderId, input.Item.Id);
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Added item {itemId} to order {orderId}")]
    private partial void LogAddedItem(string instanceId, Guid orderId, Guid itemId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to add item {itemId} to order {orderId}")]
    private partial void LogAddedItemFailed(string instanceId, Guid orderId, Guid itemId);
}

public partial class RemoveItemActivity : WorkflowActivity<RemoveItemEvent, Order>
{
    private readonly IOrderStorage _orderStorage;
    private readonly ILogger<RemoveItemActivity> _logger;
    private readonly DaprClient _daprClient;

    public RemoveItemActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<RemoveItemActivity> logger)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, RemoveItemEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null && order.State == OrderState.Creating)
        {
            var itemToRemove = order.Items.FirstOrDefault(i => i.Id == input.ItemId);
            if (itemToRemove != null)
            {
                order.Items.Remove(itemToRemove);
                await _orderStorage.UpdateOrder(order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated, order.ToDto());
                LogRemovedItem(context.InstanceId, order.Id, itemToRemove.Id);
            }
            else
            {
                // item already removed, idempotent operation
            }
        }
        else
        {
            LogRemovedItemFailed(context.InstanceId, input.OrderId, input.ItemId);
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Removed item {itemId} from order {orderId}")]
    private partial void LogRemovedItem(string instanceId, Guid orderId, Guid itemId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to remove item {itemId} from order {orderId}")]
    private partial void LogRemovedItemFailed(string instanceId, Guid orderId, Guid itemId);
}

public partial class ConfirmOrderActivity : WorkflowActivity<ConfirmOrderEvent, Order>
{
    private readonly IOrderStorage _orderStorage;
    private readonly ILogger<ConfirmOrderActivity> _logger;
    private readonly DaprClient _daprClient;

    public ConfirmOrderActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<ConfirmOrderActivity> logger)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, ConfirmOrderEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null)
        {
            if (order.State == OrderState.Creating)

            {
                order.State = OrderState.Confirmed;
                await _orderStorage.UpdateOrder(order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderConfirmed, order.ToDto());
                LogConfirmedOrder(context.InstanceId, order.Id);
            }
            else
            {
                // order already confirmed, idempotent operation
            }
        }
        else
        {
            LogConfirmedOrderFailed(context.InstanceId, input.OrderId);
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Confirmed order {orderId}")]
    private partial void LogConfirmedOrder(string instanceId, Guid orderId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to confirm order {orderId}")]
    private partial void LogConfirmedOrderFailed(string instanceId, Guid orderId);
}

public partial class ConfirmPaymentActivity : WorkflowActivity<ConfirmPaymentEvent, Order>
{
    private readonly IOrderStorage _orderStorage;
    private readonly ILogger<ConfirmPaymentActivity> _logger;
    private readonly DaprClient _daprClient;

    public ConfirmPaymentActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<ConfirmPaymentActivity> logger)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, ConfirmPaymentEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null)
        {
            if (order.State == OrderState.Confirmed)
            {
                order.State = OrderState.Paid;
                await _orderStorage.UpdateOrder(order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, order.ToDto());
                await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/newOrder", order.ToFinanceDto());
                LogPaymentConfirmed(context.InstanceId, order.Id);
            }
            else
            {
                // order already paid, idempotent operation
            }
        }
        else
        {
            LogPaymentConfirmedFailed(context.InstanceId, input.OrderId);
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Confirmed payment for order {orderId}")]
    private partial void LogPaymentConfirmed(string instanceId, Guid orderId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to confirm payment for order {orderId}")]
    private partial void LogPaymentConfirmedFailed(string instanceId, Guid orderId);
}

public partial class StartProcessingActivity : WorkflowActivity<StartProcessingEvent, Order>
{
    private readonly ILogger<StartProcessingActivity> _logger;
    private readonly IOrderStorage _orderStorage;
    private readonly DaprClient _daprClient;

    public StartProcessingActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<StartProcessingActivity> logger)
    {
        _orderStorage = orderStorage;
        _daprClient = daprClient;
        _logger = logger;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, StartProcessingEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null)
        {
            if (order.State == OrderState.Paid)
            {
                order.State = OrderState.Processing;
                await _orderStorage.UpdateOrder(order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated, order.ToDto());
                LogStartProcessing(context.InstanceId, order.Id);
            }
            else
            {
                // order already processing/processed, idempotent operation
            }
        }
        else
        {
            LogStartProcessingFailed(context.InstanceId, input.OrderId);
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Started processing order {orderId}")]
    private partial void LogStartProcessing(string instanceId, Guid orderId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to start processing order {orderId}")]
    private partial void LogStartProcessingFailed(string instanceId, Guid orderId);
}

public partial class ItemFinishedActivity : WorkflowActivity<ItemFinishedEvent, Order>
{
    private readonly ILogger<ItemFinishedActivity> _logger;
    private readonly IOrderStorage _orderStorage;
    private readonly DaprClient _daprClient;

    public ItemFinishedActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<ItemFinishedActivity> logger)
    {
        _orderStorage = orderStorage;
        _logger = logger;
        _daprClient = daprClient;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, ItemFinishedEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);

        if (order != null)
        {
            if (order.State == OrderState.Processing)
            {
                var itemToUpdate = order.Items?.FirstOrDefault(i => i.Id == input.ItemId);
                if (itemToUpdate != null)
                {
                    itemToUpdate.State = OrderItemState.Finished;

                    if (order.Items != null && order.Items.All(i => i.State == OrderItemState.Finished))
                    {
                        order.State = OrderState.Prepared;
                    }

                    await _orderStorage.UpdateOrder(order);

                    if (order.State == OrderState.Prepared)
                    {
                        await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPrepared, order.ToDto());
                    }
                    else
                    {
                        await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated, order.ToDto());
                    }

                    LogItemFinished(context.InstanceId, order.Id, itemToUpdate.Id);
                    return order;
                }
            }
        }
        else
        {
            LogItemFinishedFailed(context.InstanceId, input.OrderId, input.ItemId);
        }

        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Finished item {itemId} in order {orderId}")]
    private partial void LogItemFinished(string instanceId, Guid orderId, Guid itemId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to finish item {itemId} in order {orderId}")]
    private partial void LogItemFinishedFailed(string instanceId, Guid orderId, Guid itemId);
}

public partial class OrderServedActivity : WorkflowActivity<OrderServedEvent, Order>
{
    private readonly ILogger<OrderServedActivity> _logger;
    private readonly IOrderStorage _orderStorage;
    private readonly DaprClient _daprClient;

    public OrderServedActivity(IOrderStorage orderStorage, DaprClient daprClient, ILogger<OrderServedActivity> logger)
    {
        _orderStorage = orderStorage;
        _logger = logger;
        _daprClient = daprClient;
    }

    public override async Task<Order> RunAsync(WorkflowActivityContext context, OrderServedEvent input)
    {
        var order = await _orderStorage.GetOrderById(input.OrderId);
        if (order != null)
        {
            if (order.State == OrderState.Prepared && order.Type == OrderType.Inhouse)
            {
                order.State = OrderState.Closed;
                await _orderStorage.UpdateOrder(order);
                await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed, order.ToDto());
                await _daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/closeOrder", order.Id);
                LogServed(context.InstanceId, order.Id);
            }
            else
            {
                // order already served, idempotent operation
            }
        }
        else
        {
            LogServedFailed(context.InstanceId,input.OrderId);
        }
        return order;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] Served order {orderId}")]
    private partial void LogServed(string instanceId, Guid orderId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "[Workflow {instanceId}] Failed to serve order {orderId}")]
    private partial void LogServedFailed(string instanceId, Guid orderId);
}