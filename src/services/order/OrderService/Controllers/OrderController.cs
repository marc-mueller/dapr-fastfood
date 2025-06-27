using System.Diagnostics;
using FinanceService.Observability;
using Microsoft.AspNetCore.Mvc;
using OrderPlacement.Services;
using OrderService.Common.Dtos;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderPlacement.Controllers;

[ApiController]
[Route("api/[controller]")]
public partial class OrderController : ControllerBase
{
    private readonly ILogger<OrderController> _logger;
    private readonly IOrderProcessingService _orderProcessingService;
    private readonly IOrderServiceObservability _observability;

    public OrderController(IOrderProcessingService orderProcessingService, IOrderServiceObservability observability, ILogger<OrderController> logger)
    {
        _orderProcessingService = orderProcessingService;
        _observability = observability;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDto>> GetOrder(Guid id)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            var order = await _orderProcessingService.GetOrder(id);
            return Ok(order.ToDto());
        }
        catch
        {
            LogFailedToRetrieveOrder(id);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to retrieve order");
            return StatusCode(500, "Failed to retrieve order.");
        }
    }

    [HttpPost("createOrder")]
    public async Task<ActionResult> CreateOrder([FromBody] OrderDto orderDto)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            var order = orderDto.ToEntity();
            if (order.Id == Guid.Empty)
            {
                order.Id = Guid.NewGuid();
            }
            
            activity?.SetTag("orderId", order.Id);
            activity?.SetTag("orderType", order.Type);

            await _orderProcessingService.CreateOrder(order); // Fire and forget
            
            LogOrderCreated(order.Id, order.Type);

            return Ok(new OrderAcknowledgement { Message = "Order creation initiated", OrderId = order.Id });
        }
        catch
        {
            LogFailedToCreateOrder(orderDto.Id, orderDto.Type);
            return StatusCode(500, "Failed to initiate order creation.");
        }
    }

    [HttpPost("assignCustomer/{orderid}")]
    public async Task<ActionResult> AssignCustomer(Guid orderid, [FromBody] CustomerDto customer)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            activity?.SetTag("orderId", orderid);
            activity?.SetTag("customerId", customer.Id);
            await _orderProcessingService.AssignCustomer(orderid, customer.ToEntity()); // Fire and forget
            LogCustomerAssigned(orderid, customer.Id);
            return Ok(new OrderAcknowledgement { Message = "Customer assignment initiated", OrderId = orderid });
        }
        catch
        {
            LogFailedToAssignCustomer(orderid, customer.Id);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to assign customer");
            return StatusCode(500, "Failed to initiate customer assignment.");
        }
    }

    [HttpPost("assignInvoiceAddress/{orderid}")]
    public async Task<ActionResult> AssignInvoiceAddress(Guid orderid, [FromBody] AddressDto address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            activity?.SetTag("orderId", orderid);
            await _orderProcessingService.AssignInvoiceAddress(orderid, address.ToEntity()); // Fire and forget
            LogInvoiceAddressAssigned(orderid);
            return Ok(new OrderAcknowledgement { Message = "Invoice address assignment initiated", OrderId = orderid });
        }
        catch
        {
            LogFailedToAssignInvoiceAddress(orderid);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to assign invoice address");
            return StatusCode(500, "Failed to initiate invoice address assignment.");
        }
    }

    [HttpPost("assignDeliveryAddress/{orderid}")]
    public async Task<ActionResult> AssignDeliveryAddress(Guid orderid, [FromBody] AddressDto address)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            activity?.SetTag("orderId", orderid);
            await _orderProcessingService.AssignDeliveryAddress(orderid, address.ToEntity()); // Fire and forget
            LogDeliveryAddressAssigned(orderid);
            return Ok(new OrderAcknowledgement
                { Message = "Delivery address assignment initiated", OrderId = orderid });
        }
        catch
        {
            LogFailedToAssignDeliveryAddress(orderid);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to assign delivery address");
            return StatusCode(500, "Failed to initiate delivery address assignment.");
        }
    }

    [HttpPost("addItem/{orderid}")]
    public async Task<ActionResult> AddItem(Guid orderid, [FromBody] OrderItemDto item)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            var lineItem = item.ToEntity();
            if (lineItem.Id == Guid.Empty)
            {
                lineItem.Id = Guid.NewGuid();
            }
            activity?.SetTag("orderId", orderid);
            activity?.SetTag("itemId", lineItem.Id);
            await _orderProcessingService.AddItem(orderid, lineItem); // Fire and forget
            LogItemAdded(orderid, lineItem.Id);
            return Ok(new ItemAcknowledgement
                { Message = "Item addition initiated", OrderId = orderid, ItemId = lineItem.Id });
        }
        catch
        {
            LogFailedToAddItem(orderid, item.Id);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to add item");
            return StatusCode(500, "Failed to initiate item addition.");
        }
    }

    [HttpPost("removeItem/{orderid}")]
    public async Task<ActionResult> RemoveItem(Guid orderid, [FromBody] Guid itemId)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            activity?.SetTag("orderId", orderid);
            activity?.SetTag("itemId", itemId);
            await _orderProcessingService.RemoveItem(orderid, itemId); // Fire and forget
            LogItemRemoved(orderid, itemId);
            return Ok(new ItemAcknowledgement
                { Message = "Item removal initiated", OrderId = orderid, ItemId = itemId });
        }
        catch
        {
            LogFailedToRemoveItem(orderid, itemId);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to remove item");
            return StatusCode(500, "Failed to initiate item removal.");
        }
    }

    [HttpPost("confirmOrder/{orderid}")]
    public async Task<ActionResult> ConfirmOrder(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            activity?.SetTag("orderId", orderid);
            await _orderProcessingService.ConfirmOrder(orderid); // Fire and forget
            LogOrderConfirmed(orderid);
            return Ok(new OrderAcknowledgement { Message = "Order confirmation initiated", OrderId = orderid });
        }
        catch
        {
            LogFailedToConfirmOrder(orderid);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to confirm order");
            return StatusCode(500, "Failed to initiate order confirmation.");
        }
    }

    [HttpPost("confirmpayment/{orderid}")]
    public async Task<ActionResult> ConfirmPayment(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            activity?.SetTag("orderId", orderid);
            await _orderProcessingService.ConfirmPayment(orderid); // Fire and forget
            LogPaymentConfirmed(orderid);
            return Ok(new OrderAcknowledgement { Message = "Payment confirmation initiated", OrderId = orderid });
        }
        catch
        {
            LogFailedToConfirmPayment(orderid);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to confirm payment");
            return StatusCode(500, "Failed to initiate payment confirmation.");
        }
    }

    [HttpPost("setOrderServed/{orderid}")]
    public async Task<ActionResult> SetOrderServed(Guid orderid)
    {
        using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
        try
        {
            activity?.SetTag("orderId", orderid);
            await _orderProcessingService.Served(orderid); // Fire and forget
            LogOrderServed(orderid);
            return Ok(new OrderAcknowledgement { Message = "Order marked as served", OrderId = orderid });
        }
        catch
        {
            LogFailedToMarkOrderServed(orderid);
            activity?.SetStatus(ActivityStatusCode.Error, "Failed to mark order as served");
            return StatusCode(500, "Failed to mark order as served.");
        }
    }

    [LoggerMessage(LogLevel.Error, "Failed to retrieve order with ID {orderId}")]
    private partial void LogFailedToRetrieveOrder(Guid orderId);

    [LoggerMessage(LogLevel.Information, "Order created with ID {orderDtoId} and Type {orderDtoType}")]  
    private partial void LogOrderCreated(Guid orderDtoId, OrderType orderDtoType);
    
    [LoggerMessage(LogLevel.Error, "Failed to create order with ID {orderDtoId} and Type {orderDtoType}")]  
    private partial void LogFailedToCreateOrder(Guid orderDtoId, OrderDtoType orderDtoType);

    [LoggerMessage(LogLevel.Information, "Customer with ID {customerId} assigned to order {orderId}")]  
    private partial void LogCustomerAssigned(Guid orderId, Guid customerId);

    [LoggerMessage(LogLevel.Error, "Failed to assign customer with ID {customerId} to order {orderId}")]  
    private partial void LogFailedToAssignCustomer(Guid orderId, Guid customerId);

    [LoggerMessage(LogLevel.Information, "Invoice address assigned to order {orderId}")]  
    private partial void LogInvoiceAddressAssigned(Guid orderId);

    [LoggerMessage(LogLevel.Error, "Failed to assign invoice address to order {orderId}")]  
    private partial void LogFailedToAssignInvoiceAddress(Guid orderId);

    [LoggerMessage(LogLevel.Information, "Delivery address assigned to order {orderId}")]  
    private partial void LogDeliveryAddressAssigned(Guid orderId);

    [LoggerMessage(LogLevel.Error, "Failed to assign delivery address to order {orderId}")]  
    private partial void LogFailedToAssignDeliveryAddress(Guid orderId);

    [LoggerMessage(LogLevel.Information, "Item with ID {itemId} added to order {orderId}")]  
    private partial void LogItemAdded(Guid orderId, Guid itemId);

    [LoggerMessage(LogLevel.Error, "Failed to add item with ID {itemId} to order {orderId}")]  
    private partial void LogFailedToAddItem(Guid orderId, Guid itemId);

    [LoggerMessage(LogLevel.Information, "Item with ID {itemId} removed from order {orderId}")]  
    private partial void LogItemRemoved(Guid orderId, Guid itemId);

    [LoggerMessage(LogLevel.Error, "Failed to remove item with ID {itemId} from order {orderId}")]  
    private partial void LogFailedToRemoveItem(Guid orderId, Guid itemId);

    [LoggerMessage(LogLevel.Information, "Order {orderId} confirmed")]  
    private partial void LogOrderConfirmed(Guid orderId);

    [LoggerMessage(LogLevel.Error, "Failed to confirm order with ID {orderId}")]  
    private partial void LogFailedToConfirmOrder(Guid orderId);

    [LoggerMessage(LogLevel.Information, "Payment confirmed for order {orderId}")]  
    private partial void LogPaymentConfirmed(Guid orderId);

    [LoggerMessage(LogLevel.Error, "Failed to confirm payment for order with ID {orderId}")]  
    private partial void LogFailedToConfirmPayment(Guid orderId);

    [LoggerMessage(LogLevel.Information, "Order {orderId} marked as served")]  
    private partial void LogOrderServed(Guid orderId);

    [LoggerMessage(LogLevel.Error, "Failed to mark order with ID {orderId} as served")]  
    private partial void LogFailedToMarkOrderServed(Guid orderId);
}