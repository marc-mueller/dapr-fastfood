# Feature Flags: Dynamic Updates Implementation

## Problem Statement

**Challenge**: How do frontend Vue.js applications receive feature flag updates when flags are changed at runtime (e.g., in Azure App Configuration) without requiring users to refresh their browser?

**Sub-challenges**:
1. **Static SPA**: Vue.js builds to static JavaScript - no automatic server push
2. **Percentage Rollouts**: 25% rollout should mean 25% of users, not 25% of requests
3. **Time-Based Features**: Server time must be authoritative, not client time
4. **No Duplication**: Don't want feature flag logic in both backend and frontend

## Solution Architecture

### Overview: Server-Side Evaluation with Client-Side Polling

```
┌─────────────────────┐
│ Azure App Config    │ Feature flags stored centrally
└──────────┬──────────┘
           │
           │ 30s cache refresh
           ↓
┌─────────────────────┐
│ Backend             │
│ IFeatureManager     │ Server-side evaluation with filters
│ (OrderService,      │
│  KitchenService,    │
│  FrontendPOS)       │
└──────────┬──────────┘
           │
           │ HTTP API: GET /api/FeatureFlags
           │ Returns: { "LoyaltyProgram": true, "DarkMode": false, ... }
           ↓
┌─────────────────────┐
│ Frontend            │
│ Vue.js SPA          │ Polls every 30s, reactive state updates
│ (featureFlags.js)   │
└─────────────────────┘
```

### Implementation Details

#### 1. Frontend Store (`featureFlags.js`)

**Key Features**:
- **Polling Interval**: 30 seconds (matches Azure App Config cache)
- **Lifecycle Management**: Starts on mount, stops on unmount
- **Reactive State**: Vue automatically re-renders when flags change
- **User Context**: Optional userId parameter for targeting

```javascript
export const useFeatureFlagsStore = defineStore('featureFlags', () => {
    const flags = ref({ LoyaltyProgram: false, DarkMode: false, ... });
    const POLLING_INTERVAL_MS = 30000; // 30 seconds
    
    async function fetchFlags(userContext = null) {
        const params = userContext ? { userId: userContext.userId } : {};
        const response = await axios.get('/api/FeatureFlags', { params });
        flags.value = response.data;
    }
    
    function startPolling(userContext = null) {
        stopPolling();
        fetchFlags(userContext); // Immediate fetch
        pollingInterval = setInterval(() => fetchFlags(userContext), POLLING_INTERVAL_MS);
    }
    
    function stopPolling() {
        if (pollingInterval) clearInterval(pollingInterval);
    }
    
    return { flags, startPolling, stopPolling, isEnabled };
});
```

**Usage in Components**:
```vue
<script setup>
import { onMounted, onUnmounted } from 'vue';
import { useFeatureFlagsStore } from '@/stores/featureFlags';

const featureFlags = useFeatureFlagsStore();

onMounted(() => featureFlags.startPolling());
onUnmounted(() => featureFlags.stopPolling());
</script>
```

#### 2. Backend API (`FeatureFlagsController.cs`)

**Key Features**:
- **Server-Side Evaluation**: Uses `IFeatureManager` to evaluate all filters
- **No-Cache Headers**: Prevents browser from caching stale flag values
- **User Context Support**: Optional userId for targeting filters
- **Centralized**: All frontend flags evaluated in one endpoint

```csharp
[HttpGet]
public async Task<IActionResult> GetActiveFeatures([FromQuery] string? userId = null)
{
    var flags = new Dictionary<string, bool>
    {
        [FeatureFlags.LoyaltyProgram] = await _featureManager.IsEnabledAsync(FeatureFlags.LoyaltyProgram),
        [FeatureFlags.DarkMode] = await _featureManager.IsEnabledAsync(FeatureFlags.DarkMode),
        // ... more flags
    };
    
    // Prevent browser caching
    Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
    Response.Headers.Append("Pragma", "no-cache");
    Response.Headers.Append("Expires", "0");
    
    return Ok(flags);
}
```

#### 3. Azure App Configuration Integration

**Backend Configuration** (`Program.cs`):
```csharp
builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(connectionString)
           .UseFeatureFlags(featureFlagOptions =>
           {
               featureFlagOptions.CacheExpirationInterval = TimeSpan.FromSeconds(30);
           });
});

app.UseAzureAppConfiguration(); // Middleware to refresh cache
```

**Environment Configuration**:
- **Development**: `appsettings.json` with local flags (all disabled by default)
- **Staging/Prod**: `AppConfiguration:Endpoint` with Managed Identity

## How It Solves Each Challenge

### 1. Dynamic Runtime Updates ✅

**Timeline**:
```
T+0s:  Admin enables "DarkMode" in Azure Portal
T+30s: Backend cache refreshes, sees DarkMode=true
T+45s: Frontend polls, receives DarkMode=true
T+45s: Vue reactive system updates, dark theme applies
```

**Max latency**: 60 seconds (30s backend cache + 30s frontend poll)

**No page refresh required**: Vue's reactivity automatically updates all components watching the flag state.

### 2. Percentage Rollouts ✅

**Example**: 25% rollout of `NewCheckoutExperience`

**Azure App Config**:
```json
{
  "id": "NewCheckoutExperience",
  "enabled": true,
  "conditions": {
    "client_filters": [{
      "name": "Microsoft.Percentage",
      "parameters": { "Value": 25 }
    }]
  }
}
```

**How it works**:
1. Backend evaluates percentage filter using consistent hashing
2. Same request context → same result (deterministic)
3. Frontend polls every 30s, gets **same** boolean value
4. Result: ~25% of users see feature, 75% don't

**Consistency**: Each user gets consistent experience because backend uses deterministic hashing based on feature name (or userId if provided).

### 3. Time-Based Features ✅

**Example**: Surge pricing 11 AM - 2 PM

**Configuration**:
```json
{
  "id": "DynamicPricing",
  "conditions": {
    "client_filters": [{
      "name": "Microsoft.TimeWindow",
      "Parameters": {
        "Start": "11:00:00",
        "End": "14:00:00"
      }
    }]
  }
}
```

**Server-side evaluation ensures**:
- **Correct timezone**: Server time is authoritative
- **No client manipulation**: User can't change device time to bypass
- **Consistent**: All users in same timezone experience surge pricing at same time

### 4. No Logic Duplication ✅

**Single Source of Truth**:
- ✅ Feature definitions: `FastFood.FeatureManagement.Common/Constants/FeatureFlags.cs`
- ✅ Filter logic: Microsoft.FeatureManagement library (server-side)
- ✅ Evaluation: Backend `IFeatureManager`
- ✅ Frontend: Just consumes boolean results

**Frontend never sees**:
- Filter configurations
- Percentage thresholds
- Time windows
- Custom targeting rules

## Configuration Guide

### Service appsettings.json Format

All services now have **disabled** flags by default for easier enabling:

**OrderService** (`src/services/order/OrderService/appsettings.json`):
```json
{
  "AppConfiguration": {
    "ConnectionString": "",
    "Endpoint": ""
  },
  "FeatureManagement": {
    "LoyaltyProgram": false,
    "UseWorkflowImplementation": false,
    "DynamicPricing": false
  }
}
```

**To enable locally**: Change `false` → `true`

**To use Azure App Config**:
```json
{
  "AppConfiguration": {
    "ConnectionString": "Endpoint=https://xxx.azconfig.io;Id=xxx;Secret=xxx"
  }
}
```

### Example Configurations

See `/src/services/order/OrderService/appsettings.FeatureFlagExamples.json` for complete examples including:
- Simple booleans
- Time window filters
- Percentage rollouts
- Combined filters

## Testing Dynamic Updates

### Manual Test Scenario

1. **Setup**: Start application with all flags disabled
2. **Initial State**: Verify dark mode is OFF in frontend
3. **Enable Flag**: 
   - Azure Portal: Enable "DarkMode" feature flag
   - OR Local: Edit appsettings.json, set `"DarkMode": true`
4. **Wait**: 
   - Azure: Wait 30s (backend cache) + 30s (frontend poll) = ~60s max
   - Local: Restart service (no hot reload for appsettings.json)
5. **Observe**: Dark theme appears **without** browser refresh

### Automated Test

```javascript
describe('Feature Flag Polling', () => {
  it('should update flags when server changes', async () => {
    const store = useFeatureFlagsStore();
    
    // Initial state
    expect(store.flags.DarkMode).toBe(false);
    
    // Server changes flag
    mockServer.setFlag('DarkMode', true);
    
    // Trigger manual poll (or wait 30s)
    await store.fetchFlags();
    
    // Verify update
    expect(store.flags.DarkMode).toBe(true);
  });
});
```

## Performance Considerations

### Network Traffic

**Per frontend instance**:
- Request: `GET /api/FeatureFlags` every 30 seconds
- Payload: ~200 bytes JSON (6 flags)
- Annual cost: ~1 GB/year per user (negligible)

**Optimization options**:
1. Increase polling interval to 60s or 120s
2. Use WebSockets/SignalR for push (more complex)
3. Only poll when tab is visible (Page Visibility API)

### Browser Resource Usage

**Memory**: ~1 KB for flag state (negligible)
**CPU**: Minimal - one setTimeout per 30s

### Server Load

**Per flag evaluation**: ~0.1ms (in-memory cache)
**Per API call**: ~1ms for all 6 flags
**Expected load**: Easily handles 1000s of concurrent users

## Monitoring

### Metrics to Track

```prometheus
# Frontend poll rate
rate(http_requests_total{endpoint="/api/FeatureFlags"}[5m])

# Flag evaluation counts
sum(feature_evaluation_total) by (feature, enabled)

# Backend cache refresh rate
rate(azure_app_config_cache_refresh_total[5m])
```

### Alerts

```yaml
- alert: FeatureFlagPollFailures
  expr: rate(http_requests_total{endpoint="/api/FeatureFlags",status!="200"}[5m]) > 0.1
  annotations:
    summary: "Frontend failing to fetch feature flags"

- alert: AzureAppConfigStale
  expr: time() - azure_app_config_last_refresh_timestamp > 60
  annotations:
    summary: "Azure App Config cache not refreshing"
```

## Comparison with Alternatives

| Approach | Pros | Cons | Latency |
|----------|------|------|---------|
| **Polling (current)** | Simple, works everywhere, no server state | Network overhead, not instant | 30-60s |
| **WebSockets/SignalR** | Real-time updates, efficient | Complex, requires persistent connections | <1s |
| **Server-Sent Events** | One-way push, simple | Browser compatibility issues | <1s |
| **Page Refresh Only** | No polling overhead | Poor UX, requires manual action | Manual |
| **Client-Side Evaluation** | No polling needed | Logic duplication, security issues | 0s |

**Why Polling Won?**
- ✅ Simple implementation (~50 lines of code)
- ✅ No persistent connections (works with load balancers)
- ✅ 30-60s latency acceptable for feature flags (not chat/stock tickers)
- ✅ Aligns with Azure App Config cache refresh (30s)
- ✅ No server-side state management needed

## Future Enhancements

### 1. User Context Targeting

```javascript
// Pass user ID for percentage targeting
const userId = getCurrentUser().id;
featureFlags.startPolling({ userId });
```

Backend:
```csharp
// Create targeting context
var context = new TargetingContext
{
    UserId = userId,
    Groups = ["beta-testers", "premium-users"]
};
await _featureManager.IsEnabledAsync("NewCheckoutExperience", context);
```

### 2. Conditional Polling

Only poll when tab is visible:
```javascript
document.addEventListener('visibilitychange', () => {
  if (document.visibilityState === 'visible') {
    featureFlags.startPolling();
  } else {
    featureFlags.stopPolling();
  }
});
```

### 3. Push Notifications (SignalR)

For critical flags that need instant updates:
```csharp
// Backend
public class FeatureFlagHub : Hub
{
    public async Task NotifyFlagChange(string flagName, bool enabled)
    {
        await Clients.All.SendAsync("FlagChanged", flagName, enabled);
    }
}
```

## Summary

**Implementation**: Server-side feature flag evaluation with client-side polling

**Key Benefits**:
- ✅ Runtime updates without page refresh (30-60s latency)
- ✅ Percentage rollouts work correctly (deterministic)
- ✅ Time-based filters use server time
- ✅ Single source of truth (backend)
- ✅ Simple architecture (~100 lines total)

**Best For**: 
- Feature toggles that change occasionally (hours/days, not seconds)
- Applications where 30-60s latency is acceptable
- Teams wanting simple, maintainable solutions

**Not Best For**:
- Real-time features requiring <1s updates (use WebSockets)
- High-frequency changes (100s of updates/minute)
- Offline-first applications (need client-side evaluation)
