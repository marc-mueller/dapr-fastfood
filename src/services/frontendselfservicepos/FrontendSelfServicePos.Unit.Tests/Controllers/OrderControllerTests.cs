using System.Diagnostics;
using Dapr.Client;
using FrontendSelfServicePos.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Common.Dtos;
using FinanceService.Observability;
using System.Diagnostics.CodeAnalysis;

namespace FrontendSelfServicePos.Unit.Tests.Controllers;

public class OrderControllerTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly Mock<IFrontendSelfServicePosObservability> _observabilityMock;
    private readonly Mock<ILogger<OrderController>> _loggerMock;
    private readonly OrderController _controller;
    private readonly ActivitySource _activitySource;

    public OrderControllerTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _observabilityMock = new Mock<IFrontendSelfServicePosObservability>();
        _loggerMock = new Mock<ILogger<OrderController>>();
        _activitySource = new ActivitySource("TestActivitySource");
        
        // Setup observability to return an activity
        _observabilityMock.Setup(o => o.StartActivity(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<bool>()))
            .Returns(_activitySource.StartActivity("TestActivity"));

        _controller = new OrderController(_daprClientMock.Object, _observabilityMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new OrderDto { Id = orderId };
        
        var request = new HttpRequestMessage { Content = new StringContent("test") };
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, "orderservice", $"api/orderstate/{orderId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<OrderDto>(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(orderId, returnedOrder.Id);
    }

    [Fact]
    public async Task GetOrder_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var request = new HttpRequestMessage { Content = new StringContent("test") };
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, "orderservice", $"api/orderstate/{orderId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<OrderDto>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_ReturnsAcknowledgement()
    {
        // Arrange
        var orderDto = new OrderDto { Id = Guid.NewGuid() };
        var expectedAck = new OrderAcknowledgement { OrderId = orderDto.Id, Message = "Success" };
        
        // For requests WITH a body, the DaprClient creates the request internally
        // We just need to mock the response for any matching HttpRequestMessage
        _daprClientMock.Setup(m => m.InvokeMethodAsync<OrderAcknowledgement>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAck);

        // Act
        var result = await _controller.CreateOrder(orderDto);

        // Assert
        var returnedAck = Assert.IsType<OrderAcknowledgement>(result.Value);
        Assert.Equal(orderDto.Id, returnedAck.OrderId);
        Assert.Equal("Success", returnedAck.Message);
    }

    [Fact]
    public async Task CreateOrder_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderDto = new OrderDto { Id = Guid.NewGuid() };
        
        _daprClientMock.Setup(m => m.InvokeMethodAsync<OrderAcknowledgement>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.CreateOrder(orderDto);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task AddItem_ValidItem_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var item = new OrderItemDto { Id = Guid.NewGuid() };
        var expectedAck = new ItemAcknowledgement { ItemId = item.Id, OrderId = orderId, Message = "Success" };
        
        _daprClientMock.Setup(m => m.InvokeMethodAsync<ItemAcknowledgement>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAck);

        // Act
        var result = await _controller.AddItem(orderId, item);

        // Assert
        var returnedAck = Assert.IsType<ItemAcknowledgement>(result.Value);
        Assert.Equal(item.Id, returnedAck.ItemId);
        Assert.Equal(orderId, returnedAck.OrderId);
    }

    [Fact]
    public async Task AddItem_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var item = new OrderItemDto { Id = Guid.NewGuid() };
        
        _daprClientMock.Setup(m => m.InvokeMethodAsync<ItemAcknowledgement>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.AddItem(orderId, item);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task RemoveItem_ValidItemId_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var expectedAck = new ItemAcknowledgement { ItemId = itemId, OrderId = orderId, Message = "Success" };
        
        _daprClientMock.Setup(m => m.InvokeMethodAsync<ItemAcknowledgement>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAck);

        // Act
        var result = await _controller.RemoveItem(orderId, itemId);

        // Assert
        var returnedAck = Assert.IsType<ItemAcknowledgement>(result.Value);
        Assert.Equal(itemId, returnedAck.ItemId);
        Assert.Equal(orderId, returnedAck.OrderId);
    }

    [Fact]
    public async Task RemoveItem_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        
        _daprClientMock.Setup(m => m.InvokeMethodAsync<ItemAcknowledgement>(It.IsAny<HttpRequestMessage>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.RemoveItem(orderId, itemId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task ConfirmOrder_ValidOrderId_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedAck = new OrderAcknowledgement { OrderId = orderId, Message = "Success" };
        
        var request = new HttpRequestMessage { Content = new StringContent("test") };
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Post, "orderservice", $"api/orderstate/confirmorder/{orderId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<OrderAcknowledgement>(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAck);

        // Act
        var result = await _controller.ConfirmOrder(orderId);

        // Assert
        var returnedAck = Assert.IsType<OrderAcknowledgement>(result.Value);
        Assert.Equal(orderId, returnedAck.OrderId);
        Assert.Equal("Success", returnedAck.Message);
    }

    [Fact]
    public async Task ConfirmOrder_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var request = new HttpRequestMessage { Content = new StringContent("test") };
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Post, "orderservice", $"api/orderstate/confirmorder/{orderId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<OrderAcknowledgement>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.ConfirmOrder(orderId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task ConfirmPayment_ValidOrderId_ReturnsAcknowledgement()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedAck = new OrderAcknowledgement { OrderId = orderId, Message = "Success" };
        
        var request = new HttpRequestMessage { Content = new StringContent("test") };
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Post, "orderservice", $"api/orderstate/confirmpayment/{orderId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<OrderAcknowledgement>(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedAck);

        // Act
        var result = await _controller.ConfirmPayment(orderId);

        // Assert
        var returnedAck = Assert.IsType<OrderAcknowledgement>(result.Value);
        Assert.Equal(orderId, returnedAck.OrderId);
        Assert.Equal("Success", returnedAck.Message);
    }

    [Fact]
    public async Task ConfirmPayment_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        var request = new HttpRequestMessage { Content = new StringContent("test") };
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Post, "orderservice", $"api/orderstate/confirmpayment/{orderId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<OrderAcknowledgement>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.ConfirmPayment(orderId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
