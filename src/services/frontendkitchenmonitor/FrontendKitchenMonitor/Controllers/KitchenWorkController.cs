using System.Net;
using Dapr.Client;
using FastFood.Common;
using KitchenService.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FrontendKitchenMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KitchenWorkController : ControllerBase
{
    private readonly DaprClient _daprClient;
    private readonly ILogger<KitchenWorkController> _logger;
    private const string ApiPrefix = "api/kitchenwork";


    public KitchenWorkController(DaprClient daprClient, ILogger<KitchenWorkController> logger)
    {
        _daprClient = daprClient;
        _logger = logger;
    }
    
    // returns all pending order
    [HttpGet("pendingorders")]
    public async Task<ActionResult<IEnumerable<KitchenOrderDto>>> GetPendingOrders()
    {
        try
        {
            var order = await _daprClient.InvokeMethodAsync<IEnumerable<KitchenOrderDto>>(HttpMethod.Get, FastFoodConstants.Services.KitchenService, $"{ApiPrefix}/pendingorders");
            return Ok(order);
        }
        catch
        {
            return StatusCode(500, "Failed to retrieve order.");
        }
    }
    
    [HttpGet("pendingorder/{id}")]
    public async Task<ActionResult<KitchenOrderDto>> GetPendingOrder(Guid id)
    {
        try
        {
            var order = await _daprClient.InvokeMethodAsync<KitchenOrderDto>(HttpMethod.Get, FastFoodConstants.Services.KitchenService, $"{ApiPrefix}/pendingorder/{id}");
            return Ok(order);
        }
        catch (InvocationException ex)
        {
            if (ex.InnerException is HttpRequestException httpRequestException &&
                httpRequestException.StatusCode == HttpStatusCode.NotFound)
            {
                return NotFound("Order not found or is not pending.");
            }
            _logger.LogError(ex, "Failed to retrieve order.");
            return StatusCode(500, "Failed to retrieve order.");
        }
        catch(Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve order.");
            return StatusCode(500, "Failed to retrieve order.");
        }
    }

    // returns all pending items
    [HttpGet("pendingitems")]
    public async Task<ActionResult<IEnumerable<KitchenOrderItemDto>>> GetPendingItems()
    {
        try
        {
            var order = await _daprClient.InvokeMethodAsync<IEnumerable<KitchenOrderItemDto>>(HttpMethod.Get, FastFoodConstants.Services.KitchenService, $"{ApiPrefix}/pendingitems");
            return Ok(order);
        }
        catch
        {
            return StatusCode(500, "Failed to retrieve order.");
        }
    }

    // sets an item as finished
    [HttpPost("itemfinished/{id}")]
    public async Task<ActionResult<KitchenOrderItemDto>> SetItemAsFinished(Guid id)
    {
        try
        {
            var item = await _daprClient.InvokeMethodAsync<KitchenOrderItemDto>(HttpMethod.Post, FastFoodConstants.Services.KitchenService, $"{ApiPrefix}/itemfinished/{id}");
            return Ok(item);
        }
        catch
        {
            return StatusCode(500, "Failed to retrieve order.");
        }
    }
}