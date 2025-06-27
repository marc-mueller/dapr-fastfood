using Dapr;
using FastFood.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using OrderService.Common.Dtos;

namespace FrontendCustomerOrderStatus.Controllers;


[ApiController]
[Route("api/[controller]")]
public class FrontendCustomerOrderStatusEventHandlerController: ControllerBase
{
    private readonly ILogger<FrontendCustomerOrderStatusEventHandlerController> _logger;
    private readonly IHubContext<OrderUpdateHub> _hubContext;

    public FrontendCustomerOrderStatusEventHandlerController(IHubContext<OrderUpdateHub> hubContext, ILogger<FrontendCustomerOrderStatusEventHandlerController> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }
    
    [HttpPost("orderpaid")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid)]
    public async Task<ActionResult> OrderPaid(OrderDto order)
    {
        try
        {
            _logger.LogInformation("Order paid event received: {OrderId}", order.Id);
            // Broadcast to all clients subscribed to this orderId
            await _hubContext.Clients.Group(Constants.HubGroupCustomerOrderStatusMonitors)
                .SendAsync("ReceiveOrderUpdate", order);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order: {OrderId}", order.Id);
            return StatusCode(500);
        }
    }
    
    [HttpPost("orderprocessingupdated")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderProcessingUpdated)]
    public async Task<ActionResult> OrderUpdated(OrderDto order)
    {
        try
        {
            _logger.LogInformation("Order update event received: {OrderId}", order.Id);
            // Broadcast to all clients subscribed to this orderId
            await _hubContext.Clients.Group(Constants.HubGroupCustomerOrderStatusMonitors)
                .SendAsync("ReceiveOrderUpdate", order);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order: {OrderId}", order.Id);
            return StatusCode(500);
        }
    }
    
    [HttpPost("orderclosed")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderClosed)]
    public async Task<ActionResult> OrderCreated(OrderDto order)
    {
        try
        {
            _logger.LogInformation("Order created event received: {OrderId}", order.Id);
            // Broadcast to all clients subscribed to this orderId
            await _hubContext.Clients.Group(Constants.HubGroupCustomerOrderStatusMonitors)
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