using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Events;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Workflows;

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