using Dapr.Client;
using FastFood.Common;
using Microsoft.AspNetCore.Mvc;
using OrderService.Common.Dtos;

namespace FrontendCustomerOrderStatus.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class OrderController : ControllerBase
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<OrderController> _logger;
    private const string ApiPrefix = "api/orderstate";

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
            var order = await _daprClient.InvokeMethodAsync<OrderDto>(HttpMethod.Get, FastFoodConstants.Services.OrderService, $"{ApiPrefix}/{id}");
            return Ok(order);
        }
        catch
        {
            return StatusCode(500, "Failed to retrieve order.");
        }
    }
}