using Dapr.Client;
using FastFood.Common;
using FrontendKitchenMonitor.Controllers;
using KitchenService.Common.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;

namespace FrontendKitchenMonitor.Unit.Tests.Controllers;

public class KitchenWorkControllerTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly Mock<ILogger<KitchenWorkController>> _loggerMock;
    private readonly KitchenWorkController _controller;

    public KitchenWorkControllerTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _loggerMock = new Mock<ILogger<KitchenWorkController>>();
        _controller = new KitchenWorkController(_daprClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPendingOrders_ReturnsOrders()
    {
        // Arrange
        var expectedOrders = new List<KitchenOrderDto>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };

        var request = new HttpRequestMessage();
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, FastFoodConstants.Services.KitchenService, "api/kitchenwork/pendingorders"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<IEnumerable<KitchenOrderDto>>(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrders);

        // Act
        var result = await _controller.GetPendingOrders();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var orders = Assert.IsAssignableFrom<IEnumerable<KitchenOrderDto>>(okResult.Value);
        Assert.Equal(2, orders.Count());
    }

    [Fact]
    public async Task GetPendingOrders_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new HttpRequestMessage();
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, FastFoodConstants.Services.KitchenService, "api/kitchenwork/pendingorders"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<IEnumerable<KitchenOrderDto>>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetPendingOrders();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetPendingOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new KitchenOrderDto { Id = orderId };

        var request = new HttpRequestMessage();
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, FastFoodConstants.Services.KitchenService, $"api/kitchenwork/pendingorder/{orderId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<KitchenOrderDto>(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _controller.GetPendingOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var order = Assert.IsType<KitchenOrderDto>(okResult.Value);
        Assert.Equal(orderId, order.Id);
    }

    [Fact]
    public async Task GetPendingOrder_NotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new HttpRequestMessage();
        
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, FastFoodConstants.Services.KitchenService, $"api/kitchenwork/pendingorder/{orderId}"))
            .Returns(request);
        
        var httpRequestException = new HttpRequestException("Not found", null, HttpStatusCode.NotFound);
        var httpResponse = new HttpResponseMessage(HttpStatusCode.NotFound);
        var invocationException = new InvocationException(FastFoodConstants.Services.KitchenService, $"api/kitchenwork/pendingorder/{orderId}", httpRequestException, httpResponse);
        
        _daprClientMock.Setup(m => m.InvokeMethodAsync<KitchenOrderDto>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(invocationException);

        // Act
        var result = await _controller.GetPendingOrder(orderId);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.Equal("Order not found or is not pending.", notFoundResult.Value);
    }

    [Fact]
    public async Task GetPendingOrder_InvocationException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new HttpRequestMessage();
        
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, FastFoodConstants.Services.KitchenService, $"api/kitchenwork/pendingorder/{orderId}"))
            .Returns(request);
        
        var httpResponse = new HttpResponseMessage(HttpStatusCode.InternalServerError);
        var invocationException = new InvocationException(FastFoodConstants.Services.KitchenService, $"api/kitchenwork/pendingorder/{orderId}", new Exception("Service error"), httpResponse);
        
        _daprClientMock.Setup(m => m.InvokeMethodAsync<KitchenOrderDto>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(invocationException);

        // Act
        var result = await _controller.GetPendingOrder(orderId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetPendingOrder_GenericException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var request = new HttpRequestMessage();
        
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, FastFoodConstants.Services.KitchenService, $"api/kitchenwork/pendingorder/{orderId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<KitchenOrderDto>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetPendingOrder(orderId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task GetPendingItems_ReturnsItems()
    {
        // Arrange
        var expectedItems = new List<KitchenOrderItemDto>
        {
            new() { Id = Guid.NewGuid() },
            new() { Id = Guid.NewGuid() }
        };

        var request = new HttpRequestMessage();
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, FastFoodConstants.Services.KitchenService, "api/kitchenwork/pendingitems"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<IEnumerable<KitchenOrderItemDto>>(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItems);

        // Act
        var result = await _controller.GetPendingItems();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var items = Assert.IsAssignableFrom<IEnumerable<KitchenOrderItemDto>>(okResult.Value);
        Assert.Equal(2, items.Count());
    }

    [Fact]
    public async Task GetPendingItems_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var request = new HttpRequestMessage();
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Get, FastFoodConstants.Services.KitchenService, "api/kitchenwork/pendingitems"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<IEnumerable<KitchenOrderItemDto>>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetPendingItems();

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task SetItemAsFinished_ValidId_ReturnsItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var expectedItem = new KitchenOrderItemDto { Id = itemId };

        var request = new HttpRequestMessage();
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Post, FastFoodConstants.Services.KitchenService, $"api/kitchenwork/itemfinished/{itemId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<KitchenOrderItemDto>(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedItem);

        // Act
        var result = await _controller.SetItemAsFinished(itemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var item = Assert.IsType<KitchenOrderItemDto>(okResult.Value);
        Assert.Equal(itemId, item.Id);
    }

    [Fact]
    public async Task SetItemAsFinished_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var request = new HttpRequestMessage();
        
        _daprClientMock.Setup(m => m.CreateInvokeMethodRequest(HttpMethod.Post, FastFoodConstants.Services.KitchenService, $"api/kitchenwork/itemfinished/{itemId}"))
            .Returns(request);
        _daprClientMock.Setup(m => m.InvokeMethodAsync<KitchenOrderItemDto>(request, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.SetItemAsFinished(itemId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}