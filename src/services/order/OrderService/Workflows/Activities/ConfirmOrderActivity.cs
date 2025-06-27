using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

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