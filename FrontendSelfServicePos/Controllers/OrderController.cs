using Dapr.Client;
using FastFood.Common;
using Microsoft.AspNetCore.Mvc;
using OrderService.Common.Dtos;

namespace FrontendSelfServicePos.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class OrderController : ControllerBase
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<OrderController> _logger;
    private const string ApiPrefix = "api/orderactor";

    public OrderController(DaprClient daprClient, ILogger<OrderController> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        try
        {
            var order = await _daprClient.InvokeMethodAsync<OrderDto>(FastFoodConstants.Services.OrderService, $"{ApiPrefix}/getorder/{id}");
            return Ok(order);
        }
        catch
        {
            return StatusCode(500, "Failed to retrieve order.");
        }
    }

    [HttpPost("createOrder")]
    public async Task<ActionResult<OrderAcknowledgement>> CreateOrder([FromBody] OrderDto orderDto)
    {
        try
        {
            var ack = await _daprClient.InvokeMethodAsync<OrderDto, OrderAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/createorder", orderDto);

            return ack;
        }
        catch
        {
            return StatusCode(500, "Failed to initiate order creation.");
        }
    }

    [HttpPost("addItem/{orderid}")]
    public async Task<ActionResult<ItemAcknowledgement>> AddItem(Guid orderid, [FromBody] OrderItemDto item)
    {
        try
        {
            var itemAck = await _daprClient.InvokeMethodAsync<OrderItemDto, ItemAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/additem/{orderid}", item);
            return itemAck;
        }
        catch
        {
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
            return StatusCode(500, "Failed to initiate item removal.");
        }
    }

    [HttpPost("confirmOrder/{orderid}")]
    public async Task<ActionResult<OrderAcknowledgement>> ConfirmOrder(Guid orderid)
    {
        try
        {
            var orderAck = await _daprClient.InvokeMethodAsync<OrderAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/confirmorder/{orderid}");
            return orderAck;
        }
        catch
        {
            return StatusCode(500, "Failed to initiate order confirmation.");
        }
    }

    [HttpPost("confirmpayment/{orderid}")]
    public async Task<ActionResult<OrderAcknowledgement>> ConfirmPayment(Guid orderid)
    {
        try
        {
            var orderAck = await _daprClient.InvokeMethodAsync< OrderAcknowledgement>(HttpMethod.Post, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/confirmpayment/{orderid}");
            return orderAck;
        }
        catch
        {
            return StatusCode(500, "Failed to initiate payment confirmation.");
        }
    }
}