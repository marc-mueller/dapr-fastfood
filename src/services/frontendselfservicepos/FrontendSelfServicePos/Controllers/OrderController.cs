using System.Diagnostics;
using Dapr.Client;
using FastFood.Common;
using FinanceService.Observability;
using Microsoft.AspNetCore.Mvc;
using OrderService.Common.Dtos;

namespace FrontendSelfServicePos.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class OrderController : ControllerBase
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<OrderController> _logger;
    private readonly IFrontendSelfServicePosObservability _observability;
    private const string ApiPrefix = "api/orderstate";

    public OrderController(DaprClient daprClient, IFrontendSelfServicePosObservability observability, ILogger<OrderController> logger)
    {
        _daprClient = daprClient;
        _observability = observability;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            var order = await _daprClient.InvokeMethodAsync<OrderDto>(HttpMethod.Get, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/{id}");
            return Ok(order);
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to retrieve order.");
            LogFailedToRetrieveOrder(id);
            return StatusCode(500, "Failed to retrieve order.");
        }
    }

    [HttpPost("createOrder")]
    public async Task<ActionResult<OrderAcknowledgement>> CreateOrder([FromBody] OrderDto orderDto)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        activity?.SetBaggage("clientchannel", "kiosk");
        try
        {
            var ack = await _daprClient.InvokeMethodAsync<OrderDto, OrderAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/createorder", orderDto);

            return ack;
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to initiateorder creation."); 
            LogFailedToCreateOrder(orderDto?.Id ?? Guid.Empty);
            return StatusCode(500, "Failed to initiate order creation.");
        }
    }

    [HttpPost("addItem/{orderid}")]
    public async Task<ActionResult<ItemAcknowledgement>> AddItem(Guid orderid, [FromBody] OrderItemDto item)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            var itemAck = await _daprClient.InvokeMethodAsync<OrderItemDto, ItemAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/additem/{orderid}", item);
            return itemAck;
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to initiate item addition.");
            LogFailedToAddItem(orderid, item?.Id ?? Guid.Empty);
            return StatusCode(500, "Failed to initiate item addition.");
        }
    }

    [HttpPost("removeItem/{orderid}")]
    public async Task<ActionResult<ItemAcknowledgement>> RemoveItem(Guid orderid, [FromBody] Guid itemId)
    {
        try
        {
            var itemAck =  await _daprClient.InvokeMethodAsync<Guid, ItemAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/removeitem/{orderid}", itemId);
            return itemAck;
        }
        catch
        {
            LogFailedToRemoveItem(orderid, itemId);
            return StatusCode(500, "Failed to initiate item removal.");
        }
    }

    [HttpPost("confirmOrder/{orderid}")]
    public async Task<ActionResult<OrderAcknowledgement>> ConfirmOrder(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            var orderAck = await _daprClient.InvokeMethodAsync<OrderAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/confirmorder/{orderid}");
            return orderAck;
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to initiate order confirmation.");
            LogFailedToConfirmOrder(orderid);
            return StatusCode(500, "Failed to initiate order confirmation.");
        }
    }

    [HttpPost("confirmpayment/{orderid}")]
    public async Task<ActionResult<OrderAcknowledgement>> ConfirmPayment(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            var orderAck = await _daprClient.InvokeMethodAsync< OrderAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/confirmpayment/{orderid}");
            return orderAck;
        }
        catch
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to initiate payment confirmation.");
            LogFailedToConfirmPayment(orderid);
            return StatusCode(500, "Failed to initiate payment confirmation.");
        }
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Error, Message = "Failed to retrieve order with ID {orderId}")]
    private partial void LogFailedToRetrieveOrder(Guid orderId);

    [LoggerMessage(EventId = 2, Level = LogLevel.Error, Message = "Failed to create order with ID {orderId}")]
    private partial void LogFailedToCreateOrder(Guid orderId);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "Failed to add item {itemId} to order {orderId}")]
    private partial void LogFailedToAddItem(Guid orderId, Guid itemId);

    [LoggerMessage(EventId = 4, Level = LogLevel.Error, Message = "Failed to remove item {itemId} from order {orderId}")]
    private partial void LogFailedToRemoveItem(Guid orderId, Guid itemId);

    [LoggerMessage(EventId = 5, Level = LogLevel.Error, Message = "Failed to confirm order with ID {orderId}")]
    private partial void LogFailedToConfirmOrder(Guid orderId);

    [LoggerMessage(EventId = 6, Level = LogLevel.Error, Message = "Failed to confirm payment for order {orderId}")]
    private partial void LogFailedToConfirmPayment(Guid orderId);
}