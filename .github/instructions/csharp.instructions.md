---
applyTo: "**/*.cs"
---

# C# Coding Guidelines

## Dapr Integration

**Service invocation:**
```csharp
await daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/closeOrder", orderId);
```

**Pub/Sub publish:**
```csharp
await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid, eventData);
```

**Pub/Sub subscribe:**
```csharp
[Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid)]
public async Task<IActionResult> HandleOrderPaid([FromBody] OrderPaidEvent e)
```

**State management:**
```csharp
await _daprClient.SaveStateAsync(FastFoodConstants.StateStoreName, $"OrderProcessing-{orderId}", state);
var state = await _daprClient.GetStateAsync<OrderState>(FastFoodConstants.StateStoreName, key);
```

**Actors:**
```csharp
var proxy = ActorProxy.Create<IOrderActor>(new ActorId(orderId.ToString()), "OrderActor");
await proxy.ProcessOrderAsync(order);
```

## Observability Pattern

Every service has a dedicated observability class inheriting from `ObservabilityBase`:

```csharp
public class OrderServiceObservability : ObservabilityBase, IOrderServiceObservability
{
    public Counter<long> OrdersCreatedCounter { get; }
    public Histogram<double> OrderTotalDuration { get; }
}
```

**Usage in methods:**
```csharp
using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
_observability.OrdersCreatedCounter.Add(1, new KeyValuePair<string, object?>("orderType", order.Type));
```

## Feature Flags

Use `IObservableFeatureManager` for evaluation with automatic telemetry:

```csharp
private readonly IObservableFeatureManager _featureManager;

var enabled = await _featureManager.IsEnabledAsync(FeatureFlags.LoyaltyProgram);

// With targeting context for percentage rollouts
var context = new TargetingContext { UserId = orderId.ToString() };
var useWorkflow = await _featureManager.IsEnabledAsync(FeatureFlags.UseWorkflowImplementation, context);
```

Flag constants live in `FastFood.FeatureManagement.Common.Constants.FeatureFlags`.

## Naming Conventions

- **Services**: lowercase, no hyphens (`orderservice`, `kitchenservice`)
- **Dapr App IDs**: Match service names exactly
- **State keys**: Prefix with context (`OrderProcessing-`, `WF-Order-`, `OrderEventRouterTarget-`)
- **Event names**: lowercase, no spaces (`ordercreated`, `kitchenitemfinished`)

## Constants

Always use `FastFoodConstants` for Dapr component names and event names:
- `FastFoodConstants.PubSubName` → `"pubsub"`
- `FastFoodConstants.StateStoreName` → `"statestore"`
- `FastFoodConstants.EventNames.*` for all pub/sub topics
- `FastFoodConstants.Services.*` for service invocation targets
