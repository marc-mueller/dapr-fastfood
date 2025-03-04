using FinanceService.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace FinanceService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderFinanceController: ControllerBase
{
    private readonly ILogger<OrderFinanceController> _logger;

    public OrderFinanceController(ILogger<OrderFinanceController> logger)
    {
            _logger = logger;
    }
    
    [HttpPost("newOrder")]
    public ActionResult<OrderDto> NewOrder([FromBody] OrderDto order)
    {
        _logger.LogInformation("New order received in finance system: {OrderId}", order.Id);
        
        return Ok(order);
    }
    
    [HttpPost("closeOrder")]
    public ActionResult CloseOrder([FromBody] Guid orderId)
    {
        _logger.LogInformation("Order closed in finance system: {OrderId}", orderId);
        
        return Ok();
    }
    
    
}