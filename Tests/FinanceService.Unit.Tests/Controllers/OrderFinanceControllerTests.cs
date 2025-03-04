using FinanceService.Common.Dtos;
using FinanceService.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FinanceService.Unit.Tests.Controllers;

public class OrderFinanceControllerTests
{
    private readonly Mock<ILogger<OrderFinanceController>> _loggerMock;
    private readonly OrderFinanceController _controller;

    public OrderFinanceControllerTests()
    {
        _loggerMock = new Mock<ILogger<OrderFinanceController>>();
        _controller = new OrderFinanceController(_loggerMock.Object);
    }

    [Fact]
    public void NewOrder_ReturnsOkResult_WithSameOrder()
    {
        // Arrange
        var order = new OrderDto { Id = Guid.NewGuid() };

        // Act
        var result = _controller.NewOrder(order);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<OrderDto>(okResult.Value);
        Assert.Equal(order.Id, returnedOrder.Id);
    }

    [Fact]
    public void CloseOrder_ReturnsOkResult()
    {
        // Arrange
        var orderId = Guid.NewGuid();

        // Act
        var result = _controller.CloseOrder(orderId);

        // Assert
        Assert.IsType<OkResult>(result);
    }
}