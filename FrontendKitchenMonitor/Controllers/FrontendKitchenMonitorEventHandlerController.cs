using Dapr;
using FastFood.Common;
using KitchenService.Common.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace FrontendKitchenMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FrontendKitchenMonitorEventHandlerController: ControllerBase
{
    private readonly ILogger<FrontendKitchenMonitorEventHandlerController> _logger;
    private readonly IHubContext<KitchenWorkUpdateHub> _hubContext;

    public FrontendKitchenMonitorEventHandlerController(IHubContext<KitchenWorkUpdateHub> hubContext, ILogger<FrontendKitchenMonitorEventHandlerController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    [HttpPost("kitchenorderstartprocessing")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.KitchenOrderStartProcessing)]
    public async Task<ActionResult> KitchenOrderStartProcessing(KitchenOrderStartProcessingEvent startProcessingEvent)
    {
        try
        {
            _logger.LogInformation("Kitchen order start processing event received: {OrderId}", startProcessingEvent.OrderId);
            // Broadcast to all clients subscribed to this orderId
            await _hubContext.Clients.Group(Constants.HubGroupKitchenMonitors)
                .SendAsync("kitchenorderupdated", startProcessingEvent.OrderId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing the start processing event of kitchen order: {OrderId}", startProcessingEvent.OrderId);
            return StatusCode(500);
        }
    }
    
    [HttpPost("kitchenitemfinished")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.KitchenItemFinished)]
    public async Task<ActionResult> KitchenItemFinished(KitchenItemFinishedEvent itemFinishedEvent)
    {
        try
        {
            _logger.LogInformation("Kitchen item finished event received: {OrderId}", itemFinishedEvent.OrderId);
            // Broadcast to all clients subscribed to this orderId
            await _hubContext.Clients.Group(Constants.HubGroupKitchenMonitors)
                .SendAsync("kitchenorderupdated", itemFinishedEvent.OrderId);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing kitchen item finished event: {OrderId}/{ItemId}", itemFinishedEvent.OrderId, itemFinishedEvent.ItemId);
            return StatusCode(500);
        }
    }
}