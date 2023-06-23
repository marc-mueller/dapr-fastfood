using Dapr.Actors;
using Dapr.Actors.Client;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using OrderPlacement.Actors;
using OrderService.Common.Dtos;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly DaprClient _daprClient;

    public OrderController(ILogger<OrderController> logger, DaprClient daprClient)
    {
        _logger = logger;
        _daprClient = daprClient;
    }

    [HttpPost("createOrder")]
    public async Task<ActionResult> CreateOrder([FromBody] OrderDto order)
    {
        try
        {
            if (order.Id == Guid.Empty)
            {
                order.Id = Guid.NewGuid();
            }

            var actorId = new ActorId(order.Id.ToString());
            var proxy = ActorProxy.Create<IOrderActor>(actorId, nameof(OrderActor));
            await proxy.CreateOrder(order);

            return Ok();
        }
        catch
        {
            return StatusCode(500);
        }
    }
}