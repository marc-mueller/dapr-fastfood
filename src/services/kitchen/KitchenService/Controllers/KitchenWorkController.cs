using FinanceService.Observability;
using KitchenService.Common.Dtos;
using KitchenService.Helpers;
using KitchenService.Services;
using Microsoft.AspNetCore.Mvc;

namespace KitchenService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KitchenWorkController : ControllerBase
{
    private readonly IKitchenService _kitchenServie;
    private readonly ILogger<KitchenWorkController> _logger;
    private readonly IKitchenServiceObservability _observability;

    public KitchenWorkController(IKitchenService kitchenService, IKitchenServiceObservability observability, ILogger<KitchenWorkController> logger)
    {
        _kitchenServie = kitchenService;
        _observability = observability;
        _logger = logger;
    }

    // returns all pending order
    [HttpGet("pendingorders")]
    public async Task<ActionResult<IEnumerable<KitchenOrderDto>>> GetPendingOrders()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        _logger.LogInformation("GetPendingOrders requested");
        var orders = await _kitchenServie.GetPendingOrders();
        return Ok(orders.Select(o => o.ToDto()));
    }
    
    [HttpGet("pendingorder/{id}")]
    public async Task<ActionResult<KitchenOrderDto>> GetPendingOrder(Guid id)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        _logger.LogInformation("GetPendingOrder requested for {OrderId}", id);
        var order = await _kitchenServie.GetPendingOrder(id);
        if (order != null)
        {
            return Ok(order.ToDto());
        }
        else
        {
            return NotFound();
        }
    }

    // returns all pending items
    [HttpGet("pendingitems")]
    public async Task<ActionResult<IEnumerable<KitchenOrderItemDto>>> GetPendingItems()
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        _logger.LogInformation("GetPendingItems requested");
        var items = await _kitchenServie.GetPendingItems();
        return Ok(items.Select(i => i.ToDto()));
    }

    // sets an item as finished
    [HttpPost("itemfinished/{id}")]
    public async Task<ActionResult<KitchenOrderItemDto>> SetItemAsFinished(Guid id)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        var item = await _kitchenServie.SetItemAsFinished(id);
        _logger.LogInformation("SetItemAsFinished requested for {ItemId} of order {ItemOrderId}", item.Id, item.OrderId);
        return Ok(item.ToDto());
    }
}