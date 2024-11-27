using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

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