using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

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