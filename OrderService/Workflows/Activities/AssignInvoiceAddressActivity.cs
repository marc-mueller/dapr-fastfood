using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

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