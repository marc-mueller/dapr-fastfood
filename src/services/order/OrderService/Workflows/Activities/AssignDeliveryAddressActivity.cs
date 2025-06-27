using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

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