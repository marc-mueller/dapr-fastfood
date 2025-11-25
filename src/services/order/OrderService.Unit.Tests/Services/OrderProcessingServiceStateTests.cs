using Dapr.Client;
using FastFood.Common;
using FastFood.FeatureManagement.Common.Services;
using FinanceService.Observability;
using Microsoft.Extensions.Logging;
using Moq;
using OrderPlacement.Services;
using OrderService.Common.Dtos;
using OrderService.Models.Entities;
using OrderService.Models.Helpers;

namespace OrderService.Unit.Tests.Services;

public class OrderProcessingServiceStateTests
{
    private readonly Mock<DaprClient> _daprClientMock;
    private readonly Mock<IOrderEventRouter> _orderEventRouterMock;
    private readonly IOrderServiceObservability _observability;
    private readonly Mock<ILogger<OrderProcessingServiceState>> _loggerMock;
    private readonly Mock<IOrderPricingService> _pricingServiceMock;
    private readonly Mock<IObservableFeatureManager> _featureManagerMock;
    private readonly OrderProcessingServiceState _service;

    public OrderProcessingServiceStateTests()
    {
        _daprClientMock = new Mock<DaprClient>();
        _orderEventRouterMock = new Mock<IOrderEventRouter>();
        _observability = new OrderServiceObservability("OrderService", "OrderService");
        _loggerMock = new Mock<ILogger<OrderProcessingServiceState>>();
        _pricingServiceMock = new Mock<IOrderPricingService>();
        _featureManagerMock = new Mock<IObservableFeatureManager>();
        
        _service = new OrderProcessingServiceState(
            _daprClientMock.Object,
            _orderEventRouterMock.Object,
            _observability,
            _loggerMock.Object,
            _pricingServiceMock.Object,
            _featureManagerMock.Object
        );
    }

    [Fact]
    public async Task GetOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var expectedOrder = new Order { Id = orderId, State = OrderState.Creating };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedOrder);

        // Act
        var result = await _service.GetOrder(orderId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.Id);
        Assert.Equal(OrderState.Creating, result.State);
    }

    [Fact]
    public async Task GetOrder_NotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order)null!);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.GetOrder(orderId));
    }

    [Fact]
    public async Task CreateOrder_ValidOrder_CreatesOrderAndPublishesEvent()
    {
        // Arrange
        var order = new Order 
        { 
            Id = Guid.NewGuid(), 
            Type = OrderType.Inhouse 
        };
        
        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{order.Id}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderCreated, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _orderEventRouterMock.Setup(m => m.RegisterOrderForService(
            order.Id, 
            OrderEventRoutingTarget.OrderProcessingServiceState))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateOrder(order);

        // Assert
        Assert.Equal(OrderState.Creating, order.State);
        Assert.NotNull(order.OrderReference);
        Assert.NotEqual(default(DateTimeOffset), order.CreatedAt);
        
        _daprClientMock.Verify(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{order.Id}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()), Times.Once);

        _daprClientMock.Verify(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderCreated, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()), Times.Once);

        _orderEventRouterMock.Verify(m => m.RegisterOrderForService(
            order.Id, 
            OrderEventRoutingTarget.OrderProcessingServiceState), Times.Once);
    }

    [Fact]
    public async Task AssignCustomer_OrderInCreatingState_AssignsCustomer()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, State = OrderState.Creating };
        var customer = new Customer { FirstName = "John", LastName = "Doe" };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderUpdated, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AssignCustomer(orderId, customer);

        // Assert
        Assert.Equal(customer, order.Customer);
        _daprClientMock.Verify(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AssignCustomer_OrderNotInCreatingState_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, State = OrderState.Confirmed };
        var customer = new Customer { FirstName = "John", LastName = "Doe" };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.AssignCustomer(orderId, customer));
    }

    [Fact]
    public async Task AddItem_NewItem_AddsItemToOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order 
        { 
            Id = orderId, 
            State = OrderState.Creating, 
            Items = new List<OrderItem>() 
        };
        var newItem = new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2 };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderUpdated, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddItem(orderId, newItem);

        // Assert
        Assert.Single(order.Items);
        Assert.Contains(newItem, order.Items);
    }

    [Fact]
    public async Task AddItem_ExistingProductId_IncreasesQuantity()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var existingItem = new OrderItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 2 };
        var order = new Order 
        { 
            Id = orderId, 
            State = OrderState.Creating, 
            Items = new List<OrderItem> { existingItem } 
        };
        var newItem = new OrderItem { Id = Guid.NewGuid(), ProductId = productId, Quantity = 3 };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderUpdated, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AddItem(orderId, newItem);

        // Assert
        Assert.Single(order.Items);
        Assert.Equal(5, existingItem.Quantity);
    }

    [Fact]
    public async Task RemoveItem_ExistingItem_RemovesItem()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var itemToRemove = new OrderItem { Id = Guid.NewGuid(), ProductId = Guid.NewGuid(), Quantity = 2 };
        var order = new Order 
        { 
            Id = orderId, 
            State = OrderState.Creating, 
            Items = new List<OrderItem> { itemToRemove } 
        };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderUpdated, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoveItem(orderId, itemToRemove.Id);

        // Assert
        Assert.Empty(order.Items);
    }

    [Fact]
    public async Task ConfirmOrder_OrderInCreatingState_ConfirmsOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, State = OrderState.Creating };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderConfirmed, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ConfirmOrder(orderId);

        // Assert
        Assert.Equal(OrderState.Confirmed, order.State);
    }

    [Fact]
    public async Task ConfirmPayment_OrderInConfirmedState_ConfirmsPayment()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order 
        { 
            Id = orderId, 
            State = OrderState.Confirmed,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-5),
            Items = new List<OrderItem>
            {
                new OrderItem { ItemPrice = 10.0m, Quantity = 2 }
            }
        };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderPaid, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.InvokeMethodAsync<object>(
            It.IsAny<HttpRequestMessage>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(null!);

        _pricingServiceMock.Setup(m => m.CalculateOrderPricing(It.IsAny<Order>()))
            .ReturnsAsync(new OrderPricingBreakdown { Subtotal = 20.0m, ServiceFee = 2.0m, Total = 22.0m });

        _featureManagerMock.Setup(m => m.IsEnabledAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        await _service.ConfirmPayment(orderId);

        // Assert
        Assert.Equal(OrderState.Paid, order.State);
        Assert.NotEqual(default(DateTimeOffset), order.PaidAt);
    }

    [Fact]
    public async Task StartProcessing_OrderInPaidState_StartsProcessing()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, State = OrderState.Paid };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderProcessingUpdated, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.StartProcessing(orderId);

        // Assert
        Assert.Equal(OrderState.Processing, order.State);
        Assert.NotEqual(default(DateTimeOffset), order.StartProcessingAt);
    }

    [Fact]
    public async Task FinishedItem_LastItemFinished_TransitionsToPrepared()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var item1 = new OrderItem { Id = Guid.NewGuid(), State = OrderItemState.Finished };
        var item2 = new OrderItem { Id = Guid.NewGuid(), State = OrderItemState.AwaitingPreparation };
        var order = new Order 
        { 
            Id = orderId, 
            State = OrderState.Processing,
            StartProcessingAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            Items = new List<OrderItem> { item1, item2 }
        };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            It.IsAny<string>(), 
            It.IsAny<string>(), 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.FinishedItem(orderId, item2.Id);

        // Assert
        Assert.Equal(OrderItemState.Finished, item2.State);
        Assert.Equal(OrderState.Prepared, order.State);
        Assert.NotEqual(default(DateTimeOffset), order.PreparationFinishedAt);
        
        _daprClientMock.Verify(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderPrepared, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Served_InhouseOrderInPreparedState_ClosesOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order 
        { 
            Id = orderId, 
            State = OrderState.Prepared,
            Type = OrderType.Inhouse,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-20)
        };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderClosed, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.InvokeMethodAsync<object>(
            It.IsAny<HttpRequestMessage>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(null!);

        _orderEventRouterMock.Setup(m => m.RemoveRoutingTargetForOrder(orderId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.Served(orderId);

        // Assert
        Assert.Equal(OrderState.Closed, order.State);
        Assert.NotEqual(default(DateTimeOffset), order.ClosedAt);
        _orderEventRouterMock.Verify(m => m.RemoveRoutingTargetForOrder(orderId), Times.Once);
    }

    [Fact]
    public async Task StartDelivery_DeliveryOrderInPreparedState_StartsDelivery()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order 
        { 
            Id = orderId, 
            State = OrderState.Prepared,
            Type = OrderType.Delivery
        };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderProcessingUpdated, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.StartDelivery(orderId);

        // Assert
        Assert.Equal(OrderState.Delivering, order.State);
        Assert.NotEqual(default(DateTimeOffset), order.StartDeliveringAt);
    }

    [Fact]
    public async Task Delivered_DeliveryOrderInDeliveringState_ClosesOrder()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order 
        { 
            Id = orderId, 
            State = OrderState.Delivering,
            Type = OrderType.Delivery,
            CreatedAt = DateTimeOffset.UtcNow.AddMinutes(-30),
            StartDeliveringAt = DateTimeOffset.UtcNow.AddMinutes(-5)
        };
        
        _daprClientMock.Setup(m => m.GetStateAsync<Order>(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<ConsistencyMode?>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _daprClientMock.Setup(m => m.SaveStateAsync(
            FastFoodConstants.StateStoreName, 
            $"OrderProcessing-{orderId}", 
            It.IsAny<Order>(), 
            It.IsAny<StateOptions>(), 
            It.IsAny<IReadOnlyDictionary<string, string>>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _daprClientMock.Setup(m => m.PublishEventAsync(
            FastFoodConstants.PubSubName, 
            FastFoodConstants.EventNames.OrderClosed, 
            It.IsAny<OrderDto>(), 
            It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _orderEventRouterMock.Setup(m => m.RemoveRoutingTargetForOrder(orderId))
            .Returns(Task.CompletedTask);

        // Act
        await _service.Delivered(orderId);

        // Assert
        Assert.Equal(OrderState.Closed, order.State);
        Assert.NotEqual(default(DateTimeOffset), order.ClosedAt);
        Assert.NotEqual(default(DateTimeOffset), order.DeliveredAt);
        _orderEventRouterMock.Verify(m => m.RemoveRoutingTargetForOrder(orderId), Times.Once);
    }
}
