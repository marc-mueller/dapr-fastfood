using Dapr.Client;
using FastFood.Common;
using FrontendCustomerOrderStatus.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Common.Dtos;
using System.Net;
using Xunit;

namespace FrontendCustomerOrderStatus.Unit.Tests.Controllers;

public class OrderControllerTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly Mock<ILogger<OrderController>> _loggerMock;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _loggerMock = new Mock<ILogger<OrderController>>();
        _controller = new OrderController(_daprClientMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new OrderDto
        {
            Id = orderId,
            OrderReference = "O123",
            Type = OrderDtoType.Inhouse
        };

        var httpRequestMessage = new HttpRequestMessage();
        
        // Mock the CreateInvokeMethodRequest method
        _daprClientMock
            .Setup(d => d.CreateInvokeMethodRequest(
                HttpMethod.Get,
                FastFoodConstants.Services.OrderService,
                $"api/orderstate/{orderId}"))
            .Returns(httpRequestMessage);

        // Mock the abstract InvokeMethodAsync that takes HttpRequestMessage
        _daprClientMock
            .Setup(d => d.InvokeMethodAsync<OrderDto>(
                httpRequestMessage,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var order = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(orderId, order.Id);
        Assert.Equal("O123", order.OrderReference);
    }

    [Fact]
    public async Task GetOrder_DaprClientThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var httpRequestMessage = new HttpRequestMessage();

        // Mock the CreateInvokeMethodRequest method
        _daprClientMock
            .Setup(d => d.CreateInvokeMethodRequest(
                HttpMethod.Get,
                FastFoodConstants.Services.OrderService,
                $"api/orderstate/{orderId}"))
            .Returns(httpRequestMessage);

        // Mock the abstract InvokeMethodAsync to throw exception
        _daprClientMock
            .Setup(d => d.InvokeMethodAsync<OrderDto>(
                httpRequestMessage,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Service unavailable"));

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var statusResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, statusResult.StatusCode);
        Assert.Equal("Failed to retrieve order.", statusResult.Value);
    }

    [Fact]
    public async Task GetOrder_EmptyGuid_CallsDaprClient()
    {
        // Arrange
        var orderId = Guid.Empty;
        var expectedOrder = new OrderDto { Id = orderId };
        var httpRequestMessage = new HttpRequestMessage();

        // Mock the CreateInvokeMethodRequest method
        _daprClientMock
            .Setup(d => d.CreateInvokeMethodRequest(
                HttpMethod.Get,
                FastFoodConstants.Services.OrderService,
                $"api/orderstate/{orderId}"))
            .Returns(httpRequestMessage);

        // Mock the abstract InvokeMethodAsync
        _daprClientMock
            .Setup(d => d.InvokeMethodAsync<OrderDto>(
                httpRequestMessage,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        
        // Verify the abstract method was called
        _daprClientMock.Verify(
            d => d.InvokeMethodAsync<OrderDto>(
                httpRequestMessage,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
