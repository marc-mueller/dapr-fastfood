using FinanceService.Common.Dtos;
using FinanceService.Controllers;
using FinanceService.Observability;
using FinanceService.Services;
using FinanceService.Unit.Tests.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinanceService.Unit.Tests.Controllers;

public class OrderFinanceControllerTests
{
    private readonly Mock<ILogger<OrderFinanceController>> _loggerMock;
    private readonly Mock<IOrderFinanceService> _orderFinanceServiceMock;
    private readonly OrderFinanceController _controller;

    public OrderFinanceControllerTests()
    {
        _loggerMock = new Mock<ILogger<OrderFinanceController>>();
        _orderFinanceServiceMock = new Mock<IOrderFinanceService>();
        _controller = new OrderFinanceController(
            new TestFinanceServiceObservability(), 
            _loggerMock.Object,
            _orderFinanceServiceMock.Object);
    }

    [Fact]
    public async Task NewOrder_ReturnsOkResult_WithCreatedOrder()
    {
        // Arrange
        var order = new OrderDto { Id = Guid.NewGuid() };
        _orderFinanceServiceMock
            .Setup(s => s.CreateOrderAsync(order, default))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.NewOrder(order);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(order.Id, returnedOrder.Id);
        _orderFinanceServiceMock.Verify(s => s.CreateOrderAsync(order, default), Times.Once);
    }

    [Fact]
    public async Task CloseOrder_ReturnsOkResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderFinanceServiceMock
            .Setup(s => s.CloseOrderAsync(orderId, default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.CloseOrder(orderId);

        // Assert
        Assert.IsType<OkResult>(result);
        _orderFinanceServiceMock.Verify(s => s.CloseOrderAsync(orderId, default), Times.Once);
    }

    [Fact]
    public async Task GetOrder_ReturnsOkResult_WhenOrderExists()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new OrderDto { Id = orderId };
        _orderFinanceServiceMock
            .Setup(s => s.GetOrderAsync(orderId, default))
            .ReturnsAsync(order);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(orderId, returnedOrder.Id);
        _orderFinanceServiceMock.Verify(s => s.GetOrderAsync(orderId, default), Times.Once);
    }

    [Fact]
    public async Task GetOrder_ReturnsNotFound_WhenOrderDoesNotExist()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _orderFinanceServiceMock
            .Setup(s => s.GetOrderAsync(orderId, default))
            .ReturnsAsync((OrderDto?)null);

        // Act
        var result = await _controller.GetOrder(orderId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
        _orderFinanceServiceMock.Verify(s => s.GetOrderAsync(orderId, default), Times.Once);
    }
}