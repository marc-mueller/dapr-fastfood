# Feature Flags Demo - FastFood Delivery System

## Overview
This demo showcases the implementation of feature flags in the FastFood Delivery microservices application using **Microsoft.FeatureManagement** with local development support (appsettings.json) and Azure App Configuration for deployments.

## Architecture Summary

### Feature Flag Infrastructure
- **Common Library**: `FastFood.FeatureManagement.Common` provides shared feature flag constants, observability integration, and configuration extensions
- **Local Development**: Feature flags defined in `appsettings.json` of each service
- **Deployment**: Azure App Configuration (supports connection string and Managed Identity)
- **Observability**: Integrated with OpenTelemetry - feature evaluations and usage tracked via metrics and activity tags
- **Dynamic Updates**: 
  - **Backend**: Azure App Configuration with 30-second cache refresh
  - **Frontend**: Polling mechanism fetches flags every 30 seconds from backend API
  - **No page refresh required**: Frontend reactively updates when flags change

### Frontend Dynamic Polling Architecture

**Problem**: Vue.js SPAs are static - they don't automatically receive feature flag updates when changed in Azure App Configuration.

**Solution**: Client-side polling with server-side evaluation

1. **Frontend Store** (`featureFlags.js`):
   - Polls `/api/FeatureFlags` endpoint every 30 seconds
   - Matches Azure App Configuration cache refresh interval
   - Vue reactive state automatically updates UI when flags change
   - Cleanup on component unmount prevents memory leaks

2. **Backend API** (`FeatureFlagsController.cs`):
   - Evaluates feature flags server-side using `IFeatureManager`
   - Supports user context for targeting filters (percentage rollout)
   - Returns fresh flag values with no-cache headers
   - Azure App Config refreshes cache every 30 seconds

3. **Flow Diagram**:
   ```
   Azure App Config → (30s cache) → Backend IFeatureManager → API Endpoint
                                                                    ↓
   Frontend Poll (30s) ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ← ↓
                                                                    ↓
   Vue Reactive State → Component Re-render → UI Updates
   ```

4. **Benefits**:
   - ✅ Runtime changes reflected within 30-60 seconds
   - ✅ Percentage rollouts evaluated per-user consistently
   - ✅ Time-window filters work correctly with server time
   - ✅ No client-side flag logic duplication
   - ✅ Centralized feature flag management

### Implemented Features

| Feature | ID | Services | Description |
|---------|-----|----------|-------------|
| LoyaltyProgram | 1 | OrderService, FrontendSelfServicePos | Show/hide loyalty card UI and apply 10% discount |
| NewCheckoutExperience | 2 | FrontendSelfServicePos | A/B test alternative checkout flows |
| DarkMode | 3 | FrontendSelfServicePos | Toggle dark/light theme |
| UseWorkflowImplementation | 5 | OrderService | Route orders to Workflow instead of Actor/State |
| DynamicPricing | 6 | OrderService | Apply 1.2x surge pricing multiplier |
| AutoPrioritization | 8 | KitchenService | Automatically prioritize kitchen orders by prep time |

---

## Setup Instructions

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+ (for frontend)
- Docker & Docker Compose (for local infrastructure)
- Azure subscription (optional, for Azure App Configuration)

### Local Development Setup

1. **Start Infrastructure Services**
   ```bash
   cd infrastructure-dev
   ./StartInfrastructureServices.ps1
   ```

2. **Configure Feature Flags** (Edit service appsettings.json)
   
   Example: `src/services/order/OrderService/appsettings.json`
   ```json
   {
     "FeatureManagement": {
       "LoyaltyProgram": true,
       "UseWorkflowImplementation": false,
       "DynamicPricing": false
     }
   }
   ```

3. **Run Services**
   ```bash
   cd src
   docker compose up
   ```

### Azure App Configuration Setup (Optional)

1. **Create App Configuration Resource**
   ```bash
   az appconfig create \
     --name fastfood-featureflags-dev \
     --resource-group <your-rg> \
     --location eastus
   ```

2. **Add Feature Flags via Azure Portal or CLI**
   ```bash
   # Enable Loyalty Program
   az appconfig feature set \
     --name fastfood-featureflags-dev \
     --feature LoyaltyProgram \
     --yes
   
   az appconfig feature enable \
     --name fastfood-featureflags-dev \
     --feature LoyaltyProgram
   
   # Enable Dynamic Pricing with Time Window Filter
   az appconfig feature set \
     --name fastfood-featureflags-dev \
     --feature DynamicPricing \
     --yes \
     --label Production
   ```

3. **Configure Services** (Add connection string to environment or appsettings)
   ```json
   {
     "AppConfiguration": {
       "ConnectionString": "<connection-string-from-azure>"
     }
   }
   ```

4. **Get Connection String**
   ```bash
   az appconfig credential list \
     --name fastfood-featureflags-dev \
     --query "[0].connectionString" \
     --output tsv
   ```

---

## Demo Script

### Demo 1: Loyalty Program (Feature 1) - 5 minutes

**Goal**: Show visual UI toggle and backend discount application with observability

#### Steps:

1. **Start with Feature Disabled**
   - Edit `src/services/frontendselfservicepos/FrontendSelfServicePos/appsettings.json`:
     ```json
     "FeatureManagement": {
       "LoyaltyProgram": false
     }
     ```
   - Navigate to POS: `https://pos.localtest.me`
   - Show that no loyalty card field appears

2. **Enable Feature Locally**
   - Edit `src/services/order/OrderService/appsettings.json` and `FrontendSelfServicePos/appsettings.json`:
     ```json
     "FeatureManagement": {
       "LoyaltyProgram": true
     }
     ```
   - Restart services or wait for refresh (if using Azure App Config)

3. **Demonstrate Feature**
   - Refresh POS page
   - Show loyalty card input field appears
   - Enter loyalty number: `LOYAL12345`
   - Add items to cart and proceed to checkout
   - Confirm order

4. **Show Observability**
   - **Jaeger** (`http://jaeger.localtest.me`):
     - Search for traces with tag `feature.LoyaltyProgram.enabled=true`
     - Show trace with `feature.LoyaltyProgram.discount_applied=true`
     - Show `feature.LoyaltyProgram.discount_amount` metric
   
   - **Grafana** (`http://grafana.localtest.me`):
     - Query: `sum(feature_evaluation_total{feature="LoyaltyProgram"}) by (enabled)`
     - Query: `sum(feature_usage_total{feature="LoyaltyProgram"}) by (action)`

5. **Toggle via Azure App Configuration** (if configured)
   - Open Azure Portal → App Configuration
   - Disable `LoyaltyProgram` feature
   - Wait 30 seconds (cache expiration)
   - Refresh POS → field disappears

**Expected Results**:
- ✅ Loyalty field visibility toggles
- ✅ 10% discount applied when loyalty number provided
- ✅ Activity tags visible in Jaeger
- ✅ Metrics visible in Grafana

---

### Demo 2: Dark Mode (Feature 3) - 3 minutes

**Goal**: Simple visual theme toggle

#### Steps:

1. **Enable Dark Mode**
   - Edit `FrontendSelfServicePos/appsettings.json`:
     ```json
     "FeatureManagement": {
       "DarkMode": true
     }
     ```

2. **View Changes**
   - Refresh POS page
   - Background should be dark (gray-900)
   - Text should be light (gray-100)

3. **Disable Dark Mode**
   - Set `DarkMode: false`
   - Refresh → light theme returns

**Expected Results**:
- ✅ Theme switches between light and dark
- ✅ All components respect dark mode class

---

### Demo 3: Workflow Routing (Feature 5) - 4 minutes

**Goal**: Show backend implementation routing via feature flag

#### Steps:

1. **Check Current Routing**
   - Create an order via POS
   - Check Dapr dashboard (`http://daprdashboard.localtest.me`)
   - Show order processed via Actor or State implementation

2. **Enable Workflow Routing**
   - Edit `OrderService/appsettings.json`:
     ```json
     "FeatureManagement": {
       "UseWorkflowImplementation": true
     }
     ```
   - Restart OrderService

3. **Create New Order**
   - Place another order via POS
   - Check Dapr dashboard → Workflows tab
   - Show workflow instance created for order

4. **Show Telemetry**
   - Jaeger: Search for `feature.UseWorkflowImplementation.enabled=true`
   - Show different execution path in trace

**Expected Results**:
- ✅ Orders route to workflow implementation when enabled
- ✅ Activity tags show feature evaluation
- ✅ Workflow instances visible in Dapr dashboard

---

### Demo 4: Dynamic Pricing (Feature 6) - 5 minutes

**Goal**: Demonstrate time-based surge pricing

#### Steps:

1. **Configure Time Window**
   - Edit `OrderService/appsettings.json`:
     ```json
     "FeatureManagement": {
       "DynamicPricing": {
         "EnabledFor": [
           {
             "Name": "Microsoft.TimeWindow",
             "Parameters": {
               "Start": "11:00:00",
               "End": "14:00:00"
             }
           }
         ]
       }
     }
     ```
   - **OR** Enable always for demo:
     ```json
     "DynamicPricing": true
     ```

2. **Create Order During Peak Time**
   - Place order with items totaling $20.00
   - Check order total endpoint: `GET /api/Order/{orderId}`
   - Total should be $24.00 (1.2x multiplier)

3. **Show Pricing Calculation**
   - Jaeger trace tags:
     - `feature.DynamicPricing.enabled=true`
     - `feature.DynamicPricing.surge_applied=true`
     - `feature.DynamicPricing.surge_multiplier=1.2`
     - `feature.DynamicPricing.base_total=20.00`
     - `feature.DynamicPricing.final_total=24.00`

4. **Disable Feature**
   - Set `DynamicPricing: false`
   - Create identical order → total returns to $20.00

**Expected Results**:
- ✅ 20% price increase when feature enabled
- ✅ Detailed pricing metrics in activity tags
- ✅ Feature usage counter increments

---

### Demo 5: Kitchen Auto-Prioritization (Feature 8) - 4 minutes

**Goal**: Show intelligent queue reordering

#### Steps:

1. **Create Multiple Orders**
   - Order 1: 2x Burger, 1x Fries (est. 8min prep)
   - Order 2: 3x Drink (est. 3min prep)
   - Order 3: 1x Burger, 1x Salad (est. 7min prep)
   - Wait 2 minutes

2. **Check Kitchen Queue WITHOUT Feature**
   - Kitchen Monitor: `http://kitchen.localtest.me`
   - Items appear in creation order (FIFO)

3. **Enable Auto-Prioritization**
   - Edit `KitchenService/appsettings.json`:
     ```json
     "FeatureManagement": {
       "AutoPrioritization": true
     }
     ```
   - Restart KitchenService

4. **Check Reordered Queue**
   - Refresh Kitchen Monitor
   - Items reordered by priority formula: `wait_time / prep_time`
   - Quick items (drinks) may be deprioritized if just added
   - Items waiting longer relative to prep time rise to top

5. **Show Metrics**
   - Jaeger:
     - Tag: `feature.AutoPrioritization.queue_reordered=true`
     - Tag: `feature.AutoPrioritization.items_reordered=<count>`
   - Grafana:
     - Query: `feature_usage_total{feature="AutoPrioritization"}`

**Expected Results**:
- ✅ Queue dynamically reordered based on wait time and complexity
- ✅ Metrics track reordering events
- ✅ Kitchen staff sees optimized preparation order

---

## Observability Queries

### Grafana Dashboards

**Feature Evaluation Rate**
```promql
sum(rate(feature_evaluation_total[5m])) by (feature, enabled)
```

**Feature Usage by Service**
```promql
sum(feature_usage_total) by (service, feature)
```

**Order Duration with vs without Loyalty**
```promql
histogram_quantile(0.95, 
  sum(rate(http_server_duration_bucket[5m])) 
  by (le, feature_loyalty_enabled))
```

**Dynamic Pricing Impact**
```promql
sum(increase(feature_usage_total{feature="DynamicPricing"}[1h]))
```

### Jaeger Trace Queries

- **Search by feature tag**: `feature.LoyaltyProgram.enabled=true`
- **Find feature usage**: `feature.LoyaltyProgram.used=true`
- **Filter by action**: `feature.AutoPrioritization.action=queue_reordered`

---

## Configuration Reference

### Local Feature Flags (appsettings.json)

**OrderService**:
```json
{
  "AppConfiguration": {
    "ConnectionString": "",
    "Endpoint": ""
  },
  "FeatureManagement": {
    "LoyaltyProgram": true,
    "UseWorkflowImplementation": false,
    "DynamicPricing": false
  }
}
```

**KitchenService**:
```json
{
  "FeatureManagement": {
    "AutoPrioritization": false
  }
}
```

**FrontendSelfServicePos**:
```json
{
  "FeatureManagement": {
    "LoyaltyProgram": true,
    "NewCheckoutExperience": false,
    "DarkMode": true
  }
}
```

### Azure App Configuration Format

**Simple Boolean Flag**:
```json
{
  "id": "LoyaltyProgram",
  "enabled": true,
  "conditions": {
    "client_filters": []
  }
}
```

**Percentage-Based Rollout**:
```json
{
  "id": "NewCheckoutExperience",
  "enabled": true,
  "conditions": {
    "client_filters": [
      {
        "name": "Microsoft.Percentage",
        "parameters": {
          "Value": 25
        }
      }
    ]
  }
}
```
**Note**: Percentage rollouts are evaluated **server-side** for each frontend poll. The frontend polling mechanism ensures each user gets a consistent evaluation because:
1. Frontend polls `/api/FeatureFlags` every 30 seconds
2. Backend evaluates the percentage filter using Microsoft.FeatureManagement
3. The percentage filter uses a consistent hash of the feature name to determine which requests fall into the rollout
4. Same user gets same result across multiple polls (unless percentage changes)

This means 25% rollout = ~25% of users see the feature, not 25% of the time.

**Time Window Filter**:
```json
{
  "id": "DynamicPricing",
  "enabled": true,
  "conditions": {
    "client_filters": [
      {
        "name": "Microsoft.TimeWindow",
        "parameters": {
          "Start": "11:00:00",
          "End": "14:00:00"
        }
      }
    ]
  }
}
```

---

## Testing Feature Flags

### Unit Tests Example

```csharp
[Theory]
[InlineData(true)]
[InlineData(false)]
public async Task ConfirmOrder_WithLoyaltyProgram_AppliesDiscountCorrectly(bool featureEnabled)
{
    // Arrange
    var featureManagerMock = new Mock<IObservableFeatureManager>();
    featureManagerMock
        .Setup(x => x.IsEnabledAsync(FeatureFlags.LoyaltyProgram))
        .ReturnsAsync(featureEnabled);
    
    var order = new Order { Id = Guid.NewGuid(), Total = 100m };
    var customer = new Customer { LoyaltyNumber = "LOYAL123" };
    
    // Act
    var result = await controller.ConfirmOrder(order.Id);
    
    // Assert
    if (featureEnabled)
    {
        // Verify 10% discount applied
        Assert.Equal(90m, calculatedTotal);
    }
    else
    {
        // No discount
        Assert.Equal(100m, calculatedTotal);
    }
}
```

---

## Troubleshooting

### Feature Flags Not Updating

**Problem**: Changes in Azure App Configuration not reflected in application

**Solutions**:
1. Check cache expiration (default: 30 seconds)
2. Verify `UseAzureAppConfiguration()` middleware is registered
3. Confirm connection string/endpoint is correct
4. Check application logs for Azure App Configuration errors

### Dark Mode Not Applying

**Problem**: Dark mode class added but styles not changing

**Solutions**:
1. Verify Tailwind config has `darkMode: 'class'`
2. Check that App.vue properly applies `:class="{ 'dark': isDarkMode }"`
3. Rebuild frontend: `npm run build` in clientapp directory
4. Clear browser cache

### Observability Tags Missing

**Problem**: Feature flag tags not appearing in Jaeger

**Solutions**:
1. Ensure `IObservableFeatureManager` is used instead of `IFeatureManager`
2. Check that activity is started before feature evaluation
3. Verify observability is configured with tracing enabled
4. Check OTEL exporter endpoint is accessible

---

## Advanced Scenarios

### Managed Identity (Production)

Update `appsettings.json`:
```json
{
  "AppConfiguration": {
    "Endpoint": "https://fastfood-featureflags-prod.azconfig.io"
  }
}
```

The `FeatureManagementExtensions` will automatically use `DefaultAzureCredential` which supports:
- Azure CLI
- Visual Studio
- Managed Identity (in Azure)
- Environment variables

### User Targeting

Configure percentage filter to target specific users:
```csharp
// Custom context evaluator
public class UserContext
{
    public string UserId { get; set; }
}

// In controller
var context = new UserContext { UserId = "user123" };
var enabled = await _featureManager.IsEnabledAsync("NewCheckoutExperience", context);
```

### Kill Switch

Add emergency disable flag:
```json
{
  "FeatureManagement": {
    "EmergencyMode": false
  }
}
```

```csharp
// Check in critical paths
if (await _featureManager.IsEnabledAsync("EmergencyMode"))
{
    // Disable all non-essential features
    return;
}
```

---

## Key Takeaways

1. **Separation of Concerns**: Feature flags decouple deployment from release
2. **Observability First**: Every feature evaluation and usage is tracked
3. **Progressive Rollout**: Use percentage filters for safe deployments
4. **Local Development**: Works offline with appsettings.json
5. **Production Flexibility**: Azure App Configuration for dynamic control

---

## Resources

- **Feature Management Docs**: https://learn.microsoft.com/en-us/azure/azure-app-configuration/concept-feature-management
- **Azure App Configuration**: https://learn.microsoft.com/en-us/azure/azure-app-configuration/
- **OpenTelemetry Integration**: https://opentelemetry.io/docs/

---

**Demo Duration**: ~25 minutes (all demos)  
**Preparation Time**: ~10 minutes  
**Recommended Order**: Demo 1 → Demo 2 → Demo 4 → Demo 3 → Demo 5
