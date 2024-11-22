using Dapr.Client;
using Dapr.Workflow;
using FastFood.Common;
using OrderPlacement.Storages;
using OrderPlacement.Workflows.Extensions;
using OrderService.Models.Entities;

namespace OrderPlacement.Workflows;

public partial class CreateOrderActivity : WorkflowActivity<Guid, Order>
{
    
    private readonly ILogger<CreateOrderActivity> _logger;
    private readonly IOrderStorage _orderStorage;

    public CreateOrderActivity(IOrderStorage orderStorage, ILogger<CreateOrderActivity> logger)
    {
        _orderStorage = orderStorage;    
        _logger = logger;
    }
    
    public override async Task<Order> RunAsync(WorkflowActivityContext context, Guid input)
    {
        var order = await _orderStorage.GetOrderById(input);
        if (order == null)
        {
            order = new Order();
            order.Id = input;
            order.State = OrderState.Creating;
            await _orderStorage.UpdateOder(order);
            LogOrderCreated(context.InstanceId, order.Id);
        }
        return order;
    }
    
    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "[Workflow {instanceId}] The order {orderId} has been created.")]
    private partial void LogOrderCreated(string instanceId, Guid orderId);
}