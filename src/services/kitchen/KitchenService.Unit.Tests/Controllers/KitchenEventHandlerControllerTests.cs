using Dapr.Client;
using FinanceService.Observability;
using KitchenService.Controllers;
using KitchenService.Entities;
using KitchenService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using OrderService.Common.Dtos;
using System.Diagnostics;

namespace KitchenService.Unit.Tests.Controllers;

public class KitchenEventHandlerControllerTests
{
    private readonly Mock<IKitchenService> _kitchenServiceMock;
    private readonly Mock<IKitchenServiceObservability> _observabilityMock;
    private readonly Mock<ILogger<KitchenEventHandlerController>> _loggerMock;
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly KitchenEventHandlerController _controller;
    private readonly ActivitySource _activitySource;

    public KitchenEventHandlerControllerTests()
    {
        _kitchenServiceMock = new Mock<IKitchenService>();
        _observabilityMock = new Mock<IKitchenServiceObservability>();
        _loggerMock = new Mock<ILogger<KitchenEventHandlerController>>();
        _daprClientMock = new Mock<DaprClient>();
        _activitySource = new ActivitySource("TestActivitySource");

        _observabilityMock.Setup(o => o.StartActivity(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<bool>()))
            .Returns(_activitySource.StartActivity("TestActivity"));

        _controller = new KitchenEventHandlerController(_kitchenServiceMock.Object, _observabilityMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task NewOrder_ValidOrder_ReturnsOk()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderDto = new OrderDto
        {
            Id = orderId,
            OrderReference = "O123",
            Items = new List<OrderItemDto>
            {
                new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductDescription = "Burger", Quantity = 2 }
            }
        };

        var kitchenOrder = new KitchenOrder { Id = orderId, OrderReference = "O123" };
        _kitchenServiceMock.Setup(s => s.AddOrder(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<IEnumerable<Tuple<Guid, Guid, string, int, string?>>>()))
            .ReturnsAsync(kitchenOrder);

        // Act
        var result = await _controller.NewOrder(orderDto, _daprClientMock.Object);

        // Assert
        Assert.IsType<OkResult>(result);
        _kitchenServiceMock.Verify(s => s.AddOrder(
            orderId,
            "O123",
            It.IsAny<IEnumerable<Tuple<Guid, Guid, string, int, string?>>>()), Times.Once);
    }

    [Fact]
    public async Task NewOrder_ServiceThrowsException_ReturnsInternalServerError()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            OrderReference = "O123",
            Items = new List<OrderItemDto>
            {
                new() { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), ProductDescription = "Burger", Quantity = 2 }
            }
        };

        _kitchenServiceMock.Setup(s => s.AddOrder(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<IEnumerable<Tuple<Guid, Guid, string, int, string?>>>()))
            .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.NewOrder(orderDto, _daprClientMock.Object);

        // Assert
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }

    [Fact]
    public async Task NewOrder_NullItems_ReturnsInternalServerError()
    {
        // Arrange
        var orderDto = new OrderDto
        {
            Id = Guid.NewGuid(),
            OrderReference = "O123",
            Items = null
        };

        // Act
        var result = await _controller.NewOrder(orderDto, _daprClientMock.Object);

        // Assert
        var statusResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, statusResult.StatusCode);
    }
}
