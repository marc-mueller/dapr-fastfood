using Dapr;
using Dapr.Client;
using FastFood.Common;
using KitchenService.Services;
using Microsoft.AspNetCore.Mvc;
using OrderService.Common.Dtos;

namespace KitchenService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class KitchenEventHandlerController : ControllerBase
{
    private readonly IKitchenService _kitchenService;
    private readonly ILogger<KitchenEventHandlerController> _logger;

    public KitchenEventHandlerController(IKitchenService kitchenService, ILogger<KitchenEventHandlerController> logger)
    {
        _kitchenService = kitchenService;
        _logger = logger;
    }

    
    [HttpPost("neworder")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid)]
    public async Task<ActionResult> NewOrder(OrderDto order, [FromServices] DaprClient daprClient)
    {
        _logger.LogInformation("New order received in kitchen: {OrderId}", order.Id);
        await _kitchenService.AddOrder(order.Id, order.Items!.Select(i => new Tuple<Guid, Guid, int, string?>(i.Id, i.ProductId, i.Quantity, i.CustomerComments)));
        return Ok();
    }
}