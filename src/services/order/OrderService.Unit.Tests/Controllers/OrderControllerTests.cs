using System.Diagnostics;
using FastFood.FeatureManagement.Common.Services;
using FinanceService.Observability;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderPlacement.Controllers;
using OrderPlacement.Services;
using OrderService.Common.Dtos;
using OrderService.Models.Entities;
using Xunit;

namespace OrderService.Unit.Tests.Controllers;

public class OrderControllerTests
{
    private readonly Mock<IOrderProcessingService> _orderProcessingServiceMock;
    private readonly Mock<IOrderServiceObservability> _observabilityMock;
    private readonly Mock<ILogger<OrderController>> _loggerMock;
    private readonly Mock<IObservableFeatureManager> _featureManagerMock;
    private readonly Mock<IOrderPricingService> _pricingServiceMock;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _orderProcessingServiceMock = new Mock<IOrderProcessingService>();
        _observabilityMock = new Mock<IOrderServiceObservability>();
        _loggerMock = new Mock<ILogger<OrderController>>();
        _featureManagerMock = new Mock<IObservableFeatureManager>();
        _pricingServiceMock = new Mock<IOrderPricingService>();
        
        // Setup observability mock to return null activity (simulating no tracing)
        _observabilityMock
            .Setup(o => o.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>()))
            .Returns((Activity?)null);
        
        _controller = new OrderController(
            _orderProcessingServiceMock.Object,
            _observabilityMock.Object,
            _featureManagerMock.Object,
            _pricingServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new Order
        {
            Id = orderId,
            OrderReference = "O123",
            State = OrderState.Creating,
            Type = OrderType.Inhouse,
            Items = new List<OrderItem>()
        };
        
        _orderProcessingServiceMock
            .Setup(s => s.GetOrder(orderId))
            .ReturnsAsync(expectedOrder);

        _pricingServiceMock
            .Setup(s => s.CalculateOrderPricing(It.IsAny<Order>()))
            .ReturnsAsync(new OrderPricingBreakdown { Subtotal = 0m, ServiceFee = 0m, Total = 0m });

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orderDto = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(orderId, orderDto.Id);
        Assert.Equal("O123", orderDto.OrderReference);
        
        _orderProcessingServiceMock.Verify(s => s.GetOrder(orderId), Times.Once);
    }

    [Fact]
    public async Task GetOrder_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderProcessingServiceMock
            .Setup(s => s.GetOrder(orderId))
            .ThrowsAsync(new InvalidOperationException("Order not found"));

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Failed to retrieve order.", statusResult.Value);
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_ReturnsAcknowledgement()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.Empty, // Will be generated
            Type = OrderDtoType.Inhouse,
            Items = new List<OrderItemDto>()
        };

        _orderProcessingServiceMock
            .Setup(s => s.CreateOrder(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateOrder(orderDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<OrderAcknowledgement>(okResult.Value);
        Assert.NotEqual(Guid.Empty, ack.OrderId);
        Assert.Equal("Order creation initiated", ack.Message);
        
        _orderProcessingServiceMock.Verify(
            s => s.CreateOrder(It.Is<Order>(o => o.Id != Guid.Empty && o.Type == OrderType.Inhouse)),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrder_WithExistingId_UsesProvidedId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            Type = OrderDtoType.Delivery,
            Items = new List<OrderItemDto>()
        };

        _orderProcessingServiceMock
            .Setup(s => s.CreateOrder(It.IsAny<Order>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CreateOrder(orderDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<OrderAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
        
        _orderProcessingServiceMock.Verify(
            s => s.CreateOrder(It.Is<Order>(o => o.Id == orderId)),
            Times.Once);
    }

    [Fact]
    public async Task CreateOrder_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            Type = OrderDtoType.Inhouse,
            Items = new List<OrderItemDto>()
        };

        _orderProcessingServiceMock
            .Setup(s => s.CreateOrder(It.IsAny<Order>()))
            .ThrowsAsync(new InvalidOperationException("Creation failed"));

        // Act
        var result = await _controller.CreateOrder(orderDto);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Failed to initiate order creation.", statusResult.Value);
    }

    [Fact]
    public async Task AssignCustomer_ValidData_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerId = Guid.NewGuid();
        var customerDto = new CustomerDto
        {
            Id = customerId,
            FirstName = "John",
            LastName = "Doe"
        };

        _orderProcessingServiceMock
            .Setup(s => s.AssignCustomer(orderId, It.IsAny<Customer>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignCustomer(orderId, customerDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<OrderAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
        Assert.Equal("Customer assignment initiated", ack.Message);
        
        _orderProcessingServiceMock.Verify(
            s => s.AssignCustomer(orderId, It.Is<Customer>(c => c.Id == customerId)),
            Times.Once);
    }

    [Fact]
    public async Task AssignCustomer_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customerDto = new CustomerDto { Id = Guid.NewGuid(), FirstName = "John", LastName = "Doe" };

        _orderProcessingServiceMock
            .Setup(s => s.AssignCustomer(orderId, It.IsAny<Customer>()))
            .ThrowsAsync(new InvalidOperationException("Assignment failed"));

        // Act
        var result = await _controller.AssignCustomer(orderId, customerDto);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task AssignInvoiceAddress_ValidData_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var addressDto = new AddressDto
        {
            Street = "123 Main St",
            City = "Springfield",
            ZipCode = "12345"
        };

        _orderProcessingServiceMock
            .Setup(s => s.AssignInvoiceAddress(orderId, It.IsAny<Address>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignInvoiceAddress(orderId, addressDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<OrderAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
    }

    [Fact]
    public async Task AssignDeliveryAddress_ValidData_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var addressDto = new AddressDto
        {
            Street = "456 Oak Ave",
            City = "Springfield",
            ZipCode = "12345"
        };

        _orderProcessingServiceMock
            .Setup(s => s.AssignDeliveryAddress(orderId, It.IsAny<Address>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AssignDeliveryAddress(orderId, addressDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<OrderAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
    }

    [Fact]
    public async Task AddItem_ValidItem_ReturnsItemAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemDto = new OrderItemDto
        {
            Id = Guid.Empty, // Will be generated
            ProductId = Guid.NewGuid(),
            ProductDescription = "Burger",
            Quantity = 2,
            ItemPrice = 9.99m
        };

        _orderProcessingServiceMock
            .Setup(s => s.AddItem(orderId, It.IsAny<OrderItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddItem(orderId, itemDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<ItemAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
        Assert.NotEqual(Guid.Empty, ack.ItemId);
        
        _orderProcessingServiceMock.Verify(
            s => s.AddItem(orderId, It.Is<OrderItem>(i => i.ProductDescription == "Burger" && i.Quantity == 2)),
            Times.Once);
    }

    [Fact]
    public async Task AddItem_WithExistingItemId_UsesProvidedId()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var itemDto = new OrderItemDto
        {
            Id = itemId,
            ProductId = Guid.NewGuid(),
            ProductDescription = "Fries",
            Quantity = 1,
            ItemPrice = 2.99m
        };

        _orderProcessingServiceMock
            .Setup(s => s.AddItem(orderId, It.IsAny<OrderItem>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.AddItem(orderId, itemDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<ItemAcknowledgement>(okResult.Value);
        Assert.Equal(itemId, ack.ItemId);
    }

    [Fact]
    public async Task RemoveItem_ValidItemId_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        _orderProcessingServiceMock
            .Setup(s => s.RemoveItem(orderId, itemId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.RemoveItem(orderId, itemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<ItemAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
        Assert.Equal(itemId, ack.ItemId);
        
        _orderProcessingServiceMock.Verify(s => s.RemoveItem(orderId, itemId), Times.Once);
    }

    [Fact]
    public async Task ConfirmOrder_ValidOrderId_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderProcessingServiceMock
            .Setup(s => s.ConfirmOrder(orderId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ConfirmOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<OrderAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
        Assert.Equal("Order confirmation initiated", ack.Message);
    }

    [Fact]
    public async Task ConfirmPayment_ValidOrderId_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderProcessingServiceMock
            .Setup(s => s.ConfirmPayment(orderId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.ConfirmPayment(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<OrderAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
        Assert.Equal("Payment confirmation initiated", ack.Message);
    }

    [Fact]
    public async Task SetOrderServed_ValidOrderId_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        _orderProcessingServiceMock
            .Setup(s => s.Served(orderId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.SetOrderServed(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var ack = Assert.IsType<OrderAcknowledgement>(okResult.Value);
        Assert.Equal(orderId, ack.OrderId);
        Assert.Equal("Order marked as served", ack.Message);
    }

    [Fact]
    public async Task SetOrderServed_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderProcessingServiceMock
            .Setup(s => s.Served(orderId))
            .ThrowsAsync(new InvalidOperationException("Failed to serve order"));

        // Act
        var result = await _controller.SetOrderServed(orderId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Failed to mark order as served.", statusResult.Value);
    }
}
