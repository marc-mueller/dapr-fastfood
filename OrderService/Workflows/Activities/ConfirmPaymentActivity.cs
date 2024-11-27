using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

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