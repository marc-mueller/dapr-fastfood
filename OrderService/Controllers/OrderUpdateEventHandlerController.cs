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
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly ILogger<OrderUpdateEventHandlerController> _logger;

    public OrderUpdateEventHandlerController(IOrderProcessingService orderProcessingService, ILogger<OrderUpdateEventHandlerController> logger)
    {
            _orderProcessingService = orderProcessingService;
            _logger = logger;
    }
    
    [HttpPost("orderitemfinished")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.KitchenItemFinished)]
    public async Task<ActionResult> NewOrder(KitchenItemFinishedEvent itemEvent, [FromServices] DaprClient daprClient)
    {
        try
        {
            await _orderProcessingService.FinishedItem(itemEvent.OrderId, itemEvent.ItemId);
            _logger.LogInformation("Finished item event received for item {ItemEventItemId} in order {ItemEventOrderId}", itemEvent.ItemId, itemEvent.OrderId);
            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }
    
    [HttpPost("orderstartprocessing")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.KitchenOrderStartProcessing)]
    public async Task<ActionResult> OrderProcessing(KitchenOrderStartProcessingEvent itemEvent, [FromServices] DaprClient daprClient)
    {
        try
        {
            await _orderProcessingService.StartProcessing(itemEvent.OrderId);
            _logger.LogInformation("Start processing event received for order {ItemEventOrderId}", itemEvent.OrderId);
            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }
}