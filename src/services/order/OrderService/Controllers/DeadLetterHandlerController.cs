using Dapr;
using FastFood.Common;
using KitchenService.Common.Events;
using Microsoft.AspNetCore.Mvc;
using OrderService.Common.Dtos;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("[controller]")]
public class DeadLetterHandlerController : ControllerBase
{
    private readonly ILogger<DeadLetterHandlerController> _logger;

    public DeadLetterHandlerController(ILogger<DeadLetterHandlerController> logger)
    {
        _logger = logger;
    }

    [HttpPost("deadletter- orderpaid")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.DeadLetterOrderPaid)]
    public ActionResult HandleOrderPaidDeadLetter(OrderDto order)
    {
        _logger.LogError("Dead letter received for order paid event: {OrderId}", order.Id);
        // Additional error handling logic (e.g., alerting, retries) can be added here.
        return Ok();
    }

    [HttpPost("deadletter-kitchenitemfinished")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.DeadLetterKitchenItemFinished)]
    public ActionResult HandleKitchenItemFinishedDeadLetter(KitchenItemFinishedEvent itemEvent)
    {
        _logger.LogError("Dead letter received for kitchen item finished event: {OrderId}, {ItemId}", itemEvent.OrderId, itemEvent.ItemId);
        // Additional error handling logic can be added here.
        return Ok();
    }

    [HttpPost("deadletter-kitchenorderstartprocessing")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.DeadLetterKitchenOrderStartProcessing)]
    public ActionResult HandleKitchenOrderStartProcessingDeadLetter(KitchenOrderStartProcessingEvent itemEvent)
    {
        _logger.LogError("Dead letter received for kitchen order start processing event: {OrderId}", itemEvent.OrderId);
        // Additional error handling logic can be added here.
        return Ok();
    }
}