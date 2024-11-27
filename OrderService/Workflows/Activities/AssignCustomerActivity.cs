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