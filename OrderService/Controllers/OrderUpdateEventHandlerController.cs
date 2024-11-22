using Dapr;
using Dapr.Client;
using FastFood.Common;
using KitchenService.Common.Events;
using Microsoft.AspNetCore.Mvc;
using OrderPlacement.Services;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderUpdateEventHandlerController : ControllerBase
{
    private readonly ILogger<OrderUpdateEventHandlerController> _logger;
    private bool _failForDemo = false;
    private readonly IOrderEventRouter _orderEventRouter;
    private readonly IOrderProcessingServiceState _orderProcessingServiceState;
    private readonly IOrderProcessingServiceActor _orderProcessingServiceActor;
    private readonly IOrderProcessingServiceWorkflow _orderProcessingServiceWorkflow;

    public OrderUpdateEventHandlerController(IOrderProcessingServiceState orderProcessingServiceState, IOrderProcessingServiceActor orderProcessingServiceActor, IOrderProcessingServiceWorkflow orderProcessingServiceWorkflow, IOrderEventRouter orderEventRouter, ILogger<OrderUpdateEventHandlerController> logger)
    {
            _orderProcessingServiceState = orderProcessingServiceState;
            _orderProcessingServiceActor = orderProcessingServiceActor;
            _orderProcessingServiceWorkflow = orderProcessingServiceWorkflow;
            _orderEventRouter = orderEventRouter;
            _logger = logger;
    }
    
    private async Task<IOrderProcessingService> GetOrderProcessingServiceByOrderId(Guid orderId)
    {
        var serviceType = await _orderEventRouter.GetRoutingTargetForOrder(orderId);
        switch(serviceType)
        {
            case OrderEventRoutingTarget.OrderProcessingServiceActor:
                return _orderProcessingServiceActor;
            case OrderEventRoutingTarget.OrderProcessingServiceState:
                return _orderProcessingServiceState;
            case OrderEventRoutingTarget.OrderProcessingServiceWorkflow:
                return _orderProcessingServiceWorkflow;
            default:
                throw new Exception("Unknown service type");
        }
    }
    
    [HttpPost("orderitemfinished")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.KitchenItemFinished, DeadLetterTopic = FastFoodConstants.EventNames.DeadLetterKitchenItemFinished)]
    public async Task<ActionResult> OrderItemFinished(KitchenItemFinishedEvent itemEvent, [FromServices] DaprClient daprClient)
    {
        try
        {
            if (_failForDemo)
            {
                _logger.LogWarning("Processing failed for demo purposes");
                throw new Exception("Processing failed for demo purposes");
            }
            await (await GetOrderProcessingServiceByOrderId(itemEvent.OrderId)).FinishedItem(itemEvent.OrderId, itemEvent.ItemId);
            _logger.LogInformation("Finished item event received for item {ItemEventItemId} in order {ItemEventOrderId}", itemEvent.ItemId, itemEvent.OrderId);
            return Ok();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error processing item finished for order {OrderId} and item {ItemId}", itemEvent.OrderId, itemEvent.ItemId);
            return StatusCode(500);
        }
    }
    
    [HttpPost("orderstartprocessing")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.KitchenOrderStartProcessing, DeadLetterTopic = FastFoodConstants.EventNames.DeadLetterKitchenOrderStartProcessing)]
    public async Task<ActionResult> OrderStartProcessing(KitchenOrderStartProcessingEvent orderProcessingEvent, [FromServices] DaprClient daprClient)
    {
        try
        {
            if (_failForDemo)
            {
                _logger.LogWarning("Processing failed for demo purposes");
                throw new Exception("Processing failed for demo purposes");
            }
            await (await GetOrderProcessingServiceByOrderId(orderProcessingEvent.OrderId)).StartProcessing(orderProcessingEvent.OrderId);
            _logger.LogInformation("Start processing event received for order {ItemEventOrderId}", orderProcessingEvent.OrderId);
            return Ok();
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Error processing order start processing  for order {OrderId}", orderProcessingEvent.OrderId);
            return StatusCode(500);
        }
    }
}