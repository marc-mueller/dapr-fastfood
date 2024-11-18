using Microsoft.AspNetCore.Mvc;
using OrderPlacement.Services;
using OrderService.Common.Dtos;
using OrderService.Models.Helpers;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderProcessingService _orderProcessingService;

    public OrderController(IOrderProcessingService orderProcessingService, ILogger<OrderController> logger)
    {
        _logger = logger;
        _orderProcessingService = orderProcessingService;
    }

    // [HttpGet()]
    // public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
    // {
    //     var orders = await _orderProcessingService.GetOrders();
    //     return Ok(orders);
    // }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        var order = await _orderProcessingService.GetOrder(id);
        return Ok(order.ToDto());
    }

    // [HttpGet("activeOrders")]
    // public async Task<ActionResult<IEnumerable<OrderDto>>> GetActiveOrders()
    // {
    //     var orders = await _orderProcessingService.GetActiveOrders();
    //     return Ok(orders.Select(x => x.ToDto()));
    // }

    [HttpPost("createOrder")]
    public async Task<ActionResult<OrderDto>> CreateOrder([FromBody] OrderDto orderDto)
    {
        try
        {
            var order = orderDto.ToEntity();

            if (order.Id == Guid.Empty)
            {
                order.Id = Guid.NewGuid();
            }

            var orderResult = await _orderProcessingService.CreateOrder(order);

            return Ok(orderResult.ToDto());
        }
        catch 
        {
            return StatusCode(500);
        }
    }


    [HttpPost("assignCustomer/{orderid}")]
    public async Task<ActionResult<OrderDto>> AssignCustomer(Guid orderid, [FromBody] CustomerDto customer)
    {
        try
        {
            var orderResult = await _orderProcessingService.AssignCustomer(orderid, customer.ToEntity());

            return Ok(orderResult.ToDto());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("assignInvoiceAddress/{orderid}")]
    public async Task<ActionResult<OrderDto>> AssignInvoiceAddress(Guid orderid, [FromBody] AddressDto address)
    {
        try
        {
            var orderResult = await _orderProcessingService.AssignInvoiceAddress(orderid, address.ToEntity());

            return Ok(orderResult.ToDto());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("assignDeliveryAddress/{orderid}")]
    public async Task<ActionResult<OrderDto>> AssignDeliveryAddress(Guid orderid, [FromBody] AddressDto address)
    {
        try
        {
            var orderResult = await _orderProcessingService.AssignDeliveryAddress(orderid, address.ToEntity());

            return Ok(orderResult.ToDto());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("addItem/{orderid}")]
    public async Task<ActionResult<OrderDto>> AddItem(Guid orderid, [FromBody] OrderItemDto item)
    {
        try
        {
            var lineItem = item.ToEntity();
            if(lineItem.Id == Guid.Empty)
            {
                lineItem.Id = Guid.NewGuid();
            }
            
            var orderResult = await _orderProcessingService.AddItem(orderid, lineItem);

            return Ok(orderResult.ToDto());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("removeItem/{orderid}")]
    public async Task<ActionResult<OrderDto>> RemoveItem(Guid orderid, [FromBody] Guid itemId)
    {
        try
        {
            var orderResult = await _orderProcessingService.RemoveItem(orderid, itemId);

            return Ok(orderResult.ToDto());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("confirmOrder/{orderid}")]
    public async Task<ActionResult<OrderDto>> ConfirmOrder(Guid orderid)
    {
        try
        {
            var orderResult = await _orderProcessingService.ConfirmOrder(orderid);

            return Ok(orderResult.ToDto());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("confirmpayment/{orderid}")]
    public async Task<ActionResult<OrderDto>> ConfirmPayment(Guid orderid)
    {
        try
        {
            var orderResult = await _orderProcessingService.ConfirmPayment(orderid);

            return Ok(orderResult.ToDto());
        }
        catch
        {
            return StatusCode(500);
        }
    }

    [HttpPost("setOrderServed/{orderid}")]
    public async Task<ActionResult<OrderDto>> SetOrderServed(Guid orderid)
    {
        try
        {
            var orderResult = await _orderProcessingService.Served(orderid);

            return Ok(orderResult.ToDto());
        }
        catch
        {
            return StatusCode(500);
        }
    }
}