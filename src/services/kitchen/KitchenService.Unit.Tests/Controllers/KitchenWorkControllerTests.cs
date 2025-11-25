using FinanceService.Observability;
using KitchenService.Controllers;
using KitchenService.Entities;
using KitchenService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Diagnostics;

namespace KitchenService.Unit.Tests.Controllers;

public class KitchenWorkControllerTests
{
    private readonly Mock<IKitchenService> _kitchenServiceMock;
    private readonly Mock<IKitchenServiceObservability> _observabilityMock;
    private readonly Mock<ILogger<KitchenWorkController>> _loggerMock;
    private readonly KitchenWorkController _controller;
    private readonly ActivitySource _activitySource;

    public KitchenWorkControllerTests()
    {
        _kitchenServiceMock = new Mock<IKitchenService>();
        _observabilityMock = new Mock<IKitchenServiceObservability>();
        _loggerMock = new Mock<ILogger<KitchenWorkController>>();
        _activitySource = new ActivitySource("TestActivitySource");

        _observabilityMock.Setup(o => o.StartActivity(It.IsAny<Type>(), It.IsAny<string>(), It.IsAny<ActivityKind>(), It.IsAny<bool>()))
            .Returns(_activitySource.StartActivity("TestActivity"));

        _controller = new KitchenWorkController(_kitchenServiceMock.Object, _observabilityMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetPendingOrders_ReturnsOrders()
    {
        // Arrange
        var orders = new List<KitchenOrder>
        {
            new() { Id = Guid.NewGuid(), OrderReference = "O123" },
            new() { Id = Guid.NewGuid(), OrderReference = "O124" }
        };
        _kitchenServiceMock.Setup(s => s.GetPendingOrders()).ReturnsAsync(orders);

        // Act
        var result = await _controller.GetPendingOrders();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrders = Assert.IsAssignableFrom<IEnumerable<KitchenService.Common.Dtos.KitchenOrderDto>>(okResult.Value);
        Assert.Equal(2, returnedOrders.Count());
    }

    [Fact]
    public async Task GetPendingOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new KitchenOrder { Id = orderId, OrderReference = "O123" };
        _kitchenServiceMock.Setup(s => s.GetPendingOrder(orderId)).ReturnsAsync(order);

        // Act
        var result = await _controller.GetPendingOrder(orderId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedOrder = Assert.IsType<KitchenService.Common.Dtos.KitchenOrderDto>(okResult.Value);
        Assert.Equal(orderId, returnedOrder.Id);
    }

    [Fact]
    public async Task GetPendingOrder_NotFound_ReturnsNotFound()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        _kitchenServiceMock.Setup(s => s.GetPendingOrder(orderId)).ReturnsAsync((KitchenOrder?)null);

        // Act
        var result = await _controller.GetPendingOrder(orderId);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetPendingItems_ReturnsItems()
    {
        // Arrange
        var items = new List<KitchenOrderItem>
        {
            new() { Id = Guid.NewGuid(), OrderId = Guid.NewGuid() },
            new() { Id = Guid.NewGuid(), OrderId = Guid.NewGuid() }
        };
        _kitchenServiceMock.Setup(s => s.GetPendingItems()).ReturnsAsync(items);

        // Act
        var result = await _controller.GetPendingItems();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedItems = Assert.IsAssignableFrom<IEnumerable<KitchenService.Common.Dtos.KitchenOrderItemDto>>(okResult.Value);
        Assert.Equal(2, returnedItems.Count());
    }

    [Fact]
    public async Task SetItemAsFinished_ValidId_ReturnsItem()
    {
        // Arrange
        var itemId = Guid.NewGuid();
        var orderId = Guid.NewGuid();
        var item = new KitchenOrderItem { Id = itemId, OrderId = orderId };
        _kitchenServiceMock.Setup(s => s.SetItemAsFinished(itemId)).ReturnsAsync(item);

        // Act
        var result = await _controller.SetItemAsFinished(itemId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedItem = Assert.IsType<KitchenService.Common.Dtos.KitchenOrderItemDto>(okResult.Value);
        Assert.Equal(itemId, returnedItem.Id);
    }
}
