

using Dapr;
using FastFood.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OrderService.Common.Dtos;

namespace FrontendSelfServicePos.Controllers;


[ApiController]
[Route("api/[controller]")]
public class FrontendSelfServicePosEventHandlerController: ControllerBase
{
    private readonly ILogger<FrontendSelfServicePosEventHandlerController> _logger;
    private readonly IHubContext<OrderUpdateHub> _hubContext;

    public FrontendSelfServicePosEventHandlerController(IHubContext<OrderUpdateHub> hubContext, ILogger<FrontendSelfServicePosEventHandlerController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    [HttpPost("orderconfirmed")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderConfirmed)]
    public async Task<ActionResult> OrderConfirmed(OrderDto order)
    {
        try
        {
            // Broadcast to all clients subscribed to this orderId
            await _hubContext.Clients.Group(order.Id.ToString())
                .SendAsync("ReceiveOrderUpdate", order);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order: {OrderId}", order.Id);
            return StatusCode(500);
        }
    }
    
    [HttpPost("orderpaid")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid)]
    public async Task<ActionResult> OrderPaid(OrderDto order)
    {
        try
        {
            // Broadcast to all clients subscribed to this orderId
            await _hubContext.Clients.Group(order.Id.ToString())
                .SendAsync("ReceiveOrderUpdate", order);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order: {OrderId}", order.Id);
            return StatusCode(500);
        }
    }
}