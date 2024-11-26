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

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
        {
            try
            {
                var order = await _orderProcessingService.GetOrder(id);
                return Ok(order.ToDto());
            }
            catch
            {
                return StatusCode(500, "Failed to retrieve order.");
            }
        }

        [HttpPost("createOrder")]
        public async Task<ActionResult> CreateOrder([FromBody] OrderDto orderDto)
        {
            try
            {
                var order = orderDto.ToEntity();
                if (order.Id == Guid.Empty)
                {
                    order.Id = Guid.NewGuid();
                }

                await _orderProcessingService.CreateOrder(order); // Fire and forget

                return Ok(new OrderAcknowledgement { Message = "Order creation initiated", OrderId = order.Id });
            }
            catch
            {
                return StatusCode(500, "Failed to initiate order creation.");
            }
        }

        [HttpPost("assignCustomer/{orderid}")]
        public async Task<ActionResult> AssignCustomer(Guid orderid, [FromBody] CustomerDto customer)
        {
            try
            {
                await _orderProcessingService.AssignCustomer(orderid, customer.ToEntity()); // Fire and forget

                return Ok(new OrderAcknowledgement { Message = "Customer assignment initiated", OrderId = orderid });
            }
            catch
            {
                return StatusCode(500, "Failed to initiate customer assignment.");
            }
        }

        [HttpPost("assignInvoiceAddress/{orderid}")]
        public async Task<ActionResult> AssignInvoiceAddress(Guid orderid, [FromBody] AddressDto address)
        {
            try
            {
                await _orderProcessingService.AssignInvoiceAddress(orderid, address.ToEntity()); // Fire and forget

                return Ok(new OrderAcknowledgement{ Message = "Invoice address assignment initiated", OrderId = orderid });
            }
            catch
            {
                return StatusCode(500, "Failed to initiate invoice address assignment.");
            }
        }

        [HttpPost("assignDeliveryAddress/{orderid}")]
        public async Task<ActionResult> AssignDeliveryAddress(Guid orderid, [FromBody] AddressDto address)
        {
            try
            {
                await _orderProcessingService.AssignDeliveryAddress(orderid, address.ToEntity()); // Fire and forget

                return Ok(new OrderAcknowledgement { Message = "Delivery address assignment initiated", OrderId = orderid });
            }
            catch
            {
                return StatusCode(500, "Failed to initiate delivery address assignment.");
            }
        }

        [HttpPost("addItem/{orderid}")]
        public async Task<ActionResult> AddItem(Guid orderid, [FromBody] OrderItemDto item)
        {
            try
            {
                var lineItem = item.ToEntity();
                if (lineItem.Id == Guid.Empty)
                {
                    lineItem.Id = Guid.NewGuid();
                }

                await _orderProcessingService.AddItem(orderid, lineItem); // Fire and forget

                return Ok(new ItemAcknowledgement { Message = "Item addition initiated", OrderId = orderid, ItemId = lineItem.Id });
            }
            catch
            {
                return StatusCode(500, "Failed to initiate item addition.");
            }
        }

        [HttpPost("removeItem/{orderid}")]
        public async Task<ActionResult> RemoveItem(Guid orderid, [FromBody] Guid itemId)
        {
            try
            {
                await _orderProcessingService.RemoveItem(orderid, itemId); // Fire and forget

                return Ok(new ItemAcknowledgement { Message = "Item removal initiated", OrderId = orderid, ItemId = itemId });
            }
            catch
            {
                return StatusCode(500, "Failed to initiate item removal.");
            }
        }

        [HttpPost("confirmOrder/{orderid}")]
        public async Task<ActionResult> ConfirmOrder(Guid orderid)
        {
            try
            {
                await _orderProcessingService.ConfirmOrder(orderid); // Fire and forget

                return Ok(new OrderAcknowledgement { Message = "Order confirmation initiated", OrderId = orderid });
            }
            catch
            {
                return StatusCode(500, "Failed to initiate order confirmation.");
            }
        }

        [HttpPost("confirmpayment/{orderid}")]
        public async Task<ActionResult> ConfirmPayment(Guid orderid)
        {
            try
            {
                await _orderProcessingService.ConfirmPayment(orderid); // Fire and forget

                return Ok(new OrderAcknowledgement { Message = "Payment confirmation initiated", OrderId = orderid });
            }
            catch
            {
                return StatusCode(500, "Failed to initiate payment confirmation.");
            }
        }

        [HttpPost("setOrderServed/{orderid}")]
        public async Task<ActionResult> SetOrderServed(Guid orderid)
        {
            try
            {
                await _orderProcessingService.Served(orderid); // Fire and forget

                return Ok(new OrderAcknowledgement { Message = "Order marked as served", OrderId = orderid });
            }
            catch
            {
                return StatusCode(500, "Failed to mark order as served.");
            }
        }
}
