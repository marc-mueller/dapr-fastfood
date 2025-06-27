using FinanceService.Common.Dtos;
using FinanceService.Observability;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderFinanceController : ControllerBase
{
    private readonly ILogger<OrderFinanceController> _logger;
    private readonly IFinanceServiceObservability _observability;

    public OrderFinanceController(IFinanceServiceObservability observability, ILogger<OrderFinanceController> logger)
    {
        _observability = observability;
        _logger = logger;
    }

    [HttpPost("newOrder")]
    public ActionResult<OrderDto> NewOrder([FromBody] OrderDto order)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        _logger.LogInformation("New order received in finance system: {OrderId}", order.Id);

        return Ok(order);
    }

    [HttpPost("closeOrder")]
    public ActionResult CloseOrder([FromBody] Guid orderId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        _logger.LogInformation("Order closed in finance system: {OrderId}", orderId);

        return Ok();
    }
}