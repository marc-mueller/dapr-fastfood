using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

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