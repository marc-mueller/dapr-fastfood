using FinanceService.Common.Dtos;
using FinanceService.Observability;
using FinanceService.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderFinanceController : ControllerBase
{
    private readonly ILogger<OrderFinanceController> _logger;
    private readonly IFinanceServiceObservability _observability;
    private readonly IOrderFinanceService _orderFinanceService;

    public OrderFinanceController(
        IFinanceServiceObservability observability, 
        ILogger<OrderFinanceController> logger,
        IOrderFinanceService orderFinanceService)
    {
        _observability = observability;
        _logger = logger;
        _orderFinanceService = orderFinanceService;
    }

    [HttpPost("newOrder")]
    public async Task<ActionResult<OrderDto>> NewOrder([FromBody] OrderDto order)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        _logger.LogInformation("New order received in finance system: {OrderId}", order.Id);

        var createdOrder = await _orderFinanceService.CreateOrderAsync(order);
        return Ok(createdOrder);
    }

    [HttpPost("closeOrder")]
    public async Task<ActionResult> CloseOrder([FromBody] Guid orderId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        _logger.LogInformation("Order closed in finance system: {OrderId}", orderId);

        await _orderFinanceService.CloseOrderAsync(orderId);
        return Ok();
    }

    [HttpGet("{orderId}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid orderId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        
        var order = await _orderFinanceService.GetOrderAsync(orderId);
        
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found", orderId);
            return NotFound();
        }

        return Ok(order);
    }
}