using Dapr.Client;
using FastFood.Common;
using FastFood.FeatureManagement.Common.Services;
using FinanceService.Observability;
using KitchenService.Common.Events;
using KitchenService.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace KitchenService.Unit.Tests.Services;

public class KitchenServiceTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly IKitchenServiceObservability _observability;
    private readonly Mock<IObservableFeatureManager> _featureManagerMock;
    private readonly KitchenService.Services.KitchenService _service;

    public KitchenServiceTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _observability = new KitchenServiceObservability("KitchenService", "KitchenService");
        _featureManagerMock = new Mock<IObservableFeatureManager>();
        
        _service = new KitchenService.Services.KitchenService(
            _daprClientMock.Object,
            _observability,
            _featureManagerMock.Object
        );
    }

    [Fact]
    public async Task AddOrder_ValidOrder_AddsOrderAndPublishesEvent()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var orderReference = "O123";
        var items = new List<Tuple<Guid, Guid, string, int, string?>>
        {
            new Tuple<Guid, Guid, string, int, string?>(Guid.NewGuid(), Guid.NewGuid(), "Burger", 2, "No onions"),
            new Tuple<Guid, Guid, string, int, string?>(Guid.NewGuid(), Guid.NewGuid(), "Fries", 1, null)
        };

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName,
            FastFoodConstants.EventNames.KitchenOrderStartProcessing,
            It.IsAny<KitchenOrderStartProcessingEvent>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.AddOrder(orderId, orderReference, items);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Equal(orderReference, result.OrderReference);
        Assert.Equal(2, result.Items.Count);
        Assert.All(result.Items, item => Assert.Equal(KitchenOrderItemState.AwaitingPreparation, item.State));
        Assert.NotEqual(default(DateTimeOffset), result.CreatedAt);
        
        _daprClientMock.Verify(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName,
            FastFoodConstants.EventNames.KitchenOrderStartProcessing,
            It.Is<KitchenOrderStartProcessingEvent>(e => e.OrderId == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPendingOrders_WithPendingOrders_ReturnsOnlyPendingOrders()
    {
        // Arrange
        var orderId1 = Guid.NewGuid();
        var items1 = new List<Tuple<Guid, Guid, string, int, string?>>
        {
            new Tuple<Guid, Guid, string, int, string?>(Guid.NewGuid(), Guid.NewGuid(), "Burger", 1, null)
        };

        _daprClientMock.Setup(m => m.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.AddOrder(orderId1, "O1", items1);

        // Act
        var result = await _service.GetPendingOrders();

        // Assert
        var pendingOrders = result.ToList();
        Assert.Single(pendingOrders);
        Assert.Equal(orderId1, pendingOrders[0].Id);
    }

    [Fact]
    public async Task GetPendingOrders_NoPendingOrders_ReturnsEmpty()
    {
        // Act
        var result = await _service.GetPendingOrders();

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetPendingOrder_ExistingOrder_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var items = new List<Tuple<Guid, Guid, string, int, string?>>
        {
            new Tuple<Guid, Guid, string, int, string?>(Guid.NewGuid(), Guid.NewGuid(), "Pizza", 1, null)
        };

        _daprClientMock.Setup(m => m.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.AddOrder(orderId, "O2", items);

        // Act
        var result = await _service.GetPendingOrder(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
    }

    [Fact]
    public async Task GetPendingOrder_NonExistingOrder_ReturnsNull()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var result = await _service.GetPendingOrder(nonExistingId);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPendingItems_WithPendingItems_ReturnsAllPendingItems()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var items = new List<Tuple<Guid, Guid, string, int, string?>>
        {
            new Tuple<Guid, Guid, string, int, string?>(Guid.NewGuid(), Guid.NewGuid(), "Burger", 2, null),
            new Tuple<Guid, Guid, string, int, string?>(Guid.NewGuid(), Guid.NewGuid(), "Fries", 1, "Extra crispy")
        };

        _daprClientMock.Setup(m => m.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.AddOrder(orderId, "O3", items);

        // Act
        var result = await _service.GetPendingItems();

        // Assert
        var pendingItems = result.ToList();
        Assert.Equal(2, pendingItems.Count);
        Assert.All(pendingItems, item => Assert.Equal(KitchenOrderItemState.AwaitingPreparation, item.State));
    }

    [Fact]
    public async Task SetItemAsFinished_ValidItem_MarksItemAsFinished()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var items = new List<Tuple<Guid, Guid, string, int, string?>>
        {
            new Tuple<Guid, Guid, string, int, string?>(itemId, Guid.NewGuid(), "Burger", 1, null)
        };

        _daprClientMock.Setup(m => m.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.AddOrder(orderId, "O4", items);

        // Act
        var result = await _service.SetItemAsFinished(itemId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(itemId, result.Id);
        Assert.Equal(KitchenOrderItemState.Finished, result.State);
        Assert.NotEqual(default(DateTimeOffset), result.FinishedAt);
        
        _daprClientMock.Verify(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName,
            "kitchenitemfinished",
            It.Is<KitchenItemFinishedEvent>(e => e.ItemId == itemId && e.OrderId == orderId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetItemAsFinished_LastItemInOrder_RemovesOrderFromStorage()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        var items = new List<Tuple<Guid, Guid, string, int, string?>>
        {
            new Tuple<Guid, Guid, string, int, string?>(itemId, Guid.NewGuid(), "Burger", 1, null)
        };

        _daprClientMock.Setup(m => m.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.AddOrder(orderId, "O5", items);

        // Act
        await _service.SetItemAsFinished(itemId);

        // Assert - order should no longer be in pending orders
        var pendingOrders = await _service.GetPendingOrders();
        Assert.Empty(pendingOrders);
    }

    [Fact]
    public async Task SetItemAsFinished_OneOfMultipleItems_OrderStillPending()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemId1 = Guid.NewGuid();
        var itemId2 = Guid.NewGuid();
        var items = new List<Tuple<Guid, Guid, string, int, string?>>
        {
            new Tuple<Guid, Guid, string, int, string?>(itemId1, Guid.NewGuid(), "Burger", 1, null),
            new Tuple<Guid, Guid, string, int, string?>(itemId2, Guid.NewGuid(), "Fries", 1, null)
        };

        _daprClientMock.Setup(m => m.PublishEventAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.AddOrder(orderId, "O6", items);

        // Act
        await _service.SetItemAsFinished(itemId1);

        // Assert - order should still be in pending orders
        var pendingOrders = await _service.GetPendingOrders();
        Assert.Single(pendingOrders);
        Assert.Equal(orderId, pendingOrders.First().Id);
    }

    [Fact]
    public async Task SetItemAsFinished_NonExistingItem_ThrowsInvalidOperationException()
    {
        // Arrange
        var nonExistingItemId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SetItemAsFinished(nonExistingItemId));
    }
}
