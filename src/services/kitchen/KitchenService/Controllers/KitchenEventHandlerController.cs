using System.Diagnostics;
using Dapr;
using Dapr.Client;
using FastFood.Common;
using FinanceService.Observability;
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
    private bool _failForDemo = false;
    private readonly IKitchenServiceObservability _observability;

    public KitchenEventHandlerController(IKitchenService kitchenService, IKitchenServiceObservability observability, ILogger<KitchenEventHandlerController> logger)
    {
        _kitchenService = kitchenService;
        _observability = observability;
        _logger = logger;
    }

    
    [HttpPost("neworder")]
    [Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, DeadLetterTopic = FastFoodConstants.EventNames.DeadLetterOrderPaid)]
    public async Task<ActionResult> NewOrder(OrderDto order, [FromServices] DaprClient daprClient)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            if (_failForDemo)
            {
                _logger.LogWarning("Processing failed for demo purposes");
                throw new Exception("Processing failed for demo purposes");
            }
            _logger.LogInformation("New order received in kitchen: {OrderId}", order.Id);
            await _kitchenService.AddOrder(order.Id, order.OrderReference, order.Items!.Select(i => new Tuple<Guid, Guid, string, int, string?>(i.Id, i.ProductId, i.ProductDescription, i.Quantity, i.CustomerComments)));
            return Ok();
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Error processing new order");
            activity?.SetTag("OrderId", order.Id.ToString());
            _logger.LogError(ex, "Error processing new order: {OrderId}", order.Id);
            return StatusCode(500);
        }
    }
}