

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
            _logger.LogInformation("Order confirmed event received: {OrderId}", order.Id);
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
            _logger.LogInformation("Order paid event received: {OrderId}", order.Id);
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
    
    [HttpPost("orderupdated")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderUpdated)]
    public async Task<ActionResult> OrderUpdated(OrderDto order)
    {
        try
        {
            _logger.LogInformation("Order update event received: {OrderId}", order.Id);
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
    
    [HttpPost("ordercreated")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderCreated)]
    public async Task<ActionResult> OrderCreated(OrderDto order)
    {
        try
        {
            _logger.LogInformation("Order created event received: {OrderId}", order.Id);
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