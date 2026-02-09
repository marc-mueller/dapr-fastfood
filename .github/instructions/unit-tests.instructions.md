---
applyTo: "**/*.Unit.Tests/**/*.cs"
---

# Unit Testing Guidelines

## Test Project Structure

Tests mirror source structure: `{ServiceName}.Unit.Tests/`
- Controllers → `Controllers/{ControllerName}Tests.cs`
- Services → `Services/{ServiceName}Tests.cs`
- Helpers → `Helpers/Test{ServiceName}Observability.cs`

## Test Class Setup Pattern

```csharp
public class OrderControllerTests
{
    private readonly Mock<IOrderProcessingService> _orderProcessingServiceMock;
    private readonly Mock<IOrderServiceObservability> _observabilityMock;
    private readonly Mock<ILogger<OrderController>> _loggerMock;
    private readonly Mock<IObservableFeatureManager> _featureManagerMock;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        _orderProcessingServiceMock = new Mock<IOrderProcessingService>();
        _observabilityMock = new Mock<IOrderServiceObservability>();
        _loggerMock = new Mock<ILogger<OrderController>>();
        _featureManagerMock = new Mock<IObservableFeatureManager>();
        
        // Setup observability mock - return null activity (no tracing in tests)
        _observabilityMock
            .Setup(o => o.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>()))
            .Returns((Activity?)null);
        
        _controller = new OrderController(
            _orderProcessingServiceMock.Object,
            _observabilityMock.Object,
            _featureManagerMock.Object,
            _loggerMock.Object);
    }
}
```

## Mocking Dapr Clients

```csharp
private readonly Mock<DaprClient> _daprClientMock;

// Mock pub/sub
_daprClientMock.Setup(m => m.PublishEventAsync(
    FastFoodConstants.PubSubName,
    FastFoodConstants.EventNames.KitchenOrderStartProcessing,
    It.IsAny<KitchenOrderStartProcessingEvent>(),
    It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);

// Verify pub/sub calls
_daprClientMock.Verify(m => m.PublishEventAsync(
    FastFoodConstants.PubSubName,
    FastFoodConstants.EventNames.KitchenOrderStartProcessing,
    It.Is<KitchenOrderStartProcessingEvent>(e => e.OrderId == orderId),
    It.IsAny<CancellationToken>()), Times.Once);
```

## Test Naming Convention

`{MethodName}_{Scenario}_{ExpectedBehavior}`

```csharp
[Fact]
public async Task GetOrder_ValidId_ReturnsOrder()

[Fact]
public async Task GetOrder_ServiceThrowsException_ReturnsInternalServerError()

[Fact]
public async Task AddOrder_ValidOrder_AddsOrderAndPublishesEvent()
```

## Feature Flag Testing

Always test both enabled and disabled states:

```csharp
[Fact]
public async Task CreateOrder_LoyaltyProgramEnabled_AppliesDiscount()
{
    _featureManagerMock
        .Setup(f => f.IsEnabledAsync(FeatureFlags.LoyaltyProgram))
        .ReturnsAsync(true);
    // ...
}

[Fact]
public async Task CreateOrder_LoyaltyProgramDisabled_NoDiscount()
{
    _featureManagerMock
        .Setup(f => f.IsEnabledAsync(FeatureFlags.LoyaltyProgram))
        .ReturnsAsync(false);
    // ...
}
```

## Assertion Patterns

```csharp
// Controller results
var okResult = Assert.IsType<OkObjectResult>(result.Result);
var orderDto = Assert.IsType<OrderDto>(okResult.Value);
Assert.Equal(orderId, orderDto.Id);

// Collections
Assert.Equal(2, result.Items.Count);
Assert.All(result.Items, item => Assert.Equal(KitchenOrderItemState.AwaitingPreparation, item.State));

// Not default values
Assert.NotEqual(default(DateTimeOffset), result.CreatedAt);
```

## Observability in Tests

Create test-specific observability implementations or mock with null activities:

```csharp
// Option 1: Mock returning null
_observabilityMock
    .Setup(o => o.StartActivity(It.IsAny<string>(), It.IsAny<ActivityKind>()))
    .Returns((Activity?)null);

// Option 2: Real implementation for integration-like tests
_observability = new KitchenServiceObservability("KitchenService", "KitchenService");
```
