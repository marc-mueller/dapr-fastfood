
# FastFood Delivery - AI Coding Agent Instructions

## System Overview

This is a **microservices-based fast food delivery system** demonstrating Dapr patterns, observability, and Azure DevOps CI/CD. The architecture uses **Dapr 1.16.0** for service-to-service communication, state management, pub/sub messaging, and actors.

### Core Services
- **OrderService** (`src/services/order/`) - Order management with three implementation patterns (see Architecture Patterns below)
- **OrderService.Actors** - Separate service hosting Dapr actors for order state management
- **KitchenService** (`src/services/kitchen/`) - Kitchen order processing
- **FinanceService** (`src/services/finance/`) - Payment and financial operations
- **Frontend Services** - Vue.js 3 + Vite SPAs:
  - `FrontendSelfServicePos` - Self-service point-of-sale
  - `FrontendKitchenMonitor` - Kitchen display system
  - `FrontendCustomerOrderStatus` - Customer order tracking

## Critical Architecture Patterns

### Triple Implementation Strategy (OrderService)
The OrderService implements order lifecycle logic in **three different patterns** as a learning/demo showcase:

1. **Actor-based** (`OrderProcessingServiceActor`) - Uses Dapr Actors via separate `orderserviceactors` service
2. **State-based** (`OrderProcessingServiceState`) - Direct Dapr state store + pub/sub
3. **Workflow-based** (`OrderProcessingServiceWorkflow`) - Dapr Workflows with activities and events

**Order Routing**: Each order is assigned to ONE implementation via `OrderEventRouter`, which stores routing metadata in Redis state store (`OrderEventRouterTarget-{orderId}`). Kitchen event handlers query this router to dispatch to the correct implementation.

### Dapr Integration Points

**Service Communication:**
- HTTP/gRPC via Dapr sidecars (ports defined per service: `DAPR_HTTP_PORT=3500`, `DAPR_GRPC_PORT=50001`)
- Service invocation: `daprClient.InvokeMethodAsync(HttpMethod.Post, FastFoodConstants.Services.FinanceService, "api/OrderFinance/closeOrder", orderId)`

**Pub/Sub (RabbitMQ):**
- Component: `pubsub` (see `infrastructure-dev/dapr/resources/pubsub.yaml`)
- Event names in `FastFoodConstants.EventNames` (e.g., `OrderPaid`, `KitchenItemFinished`)
- Subscribe: `[Topic(FastFoodConstants.PubSubName, FastFoodConstants.EventNames.OrderPaid)]`
- Publish: `await _daprClient.PublishEventAsync(FastFoodConstants.PubSubName, eventName, data)`

**State Management (Redis):**
- Component: `statestore` with actor support enabled
- State keys follow patterns: `OrderProcessing-{orderId}`, `WF-Order-{orderId}`, `OrderEventRouterTarget-{orderId}`

**Actors:**
- Actors run in separate `OrderService.Actors` project/service for isolation
- Created via: `ActorProxy.Create<IOrderActor>(new ActorId(orderId.ToString()), "OrderActor")`

**Workflows:**
- Main workflow: `OrderProcessingWorkflow` coordinates order lifecycle
- External events raised via: `await _daprWorkflowClient.RaiseEventAsync(orderId.ToString(), eventName, eventData)`
- Activities named like `CreateOrderActivity`, `ConfirmPaymentActivity`

## Development Workflows

### Local Development (Docker Compose)
```bash
# Start infrastructure only (from infrastructure-dev/)
./StartInfrastructureServices.ps1  # Starts RabbitMQ, Redis, placement, Dapr dashboard

# OR run full stack (from src/)
docker compose up

# Access points:
# - Traefik proxy handles HTTPS with self-signed certs
# - Services: pos.localtest.me, kitchen.localtest.me, orderstatus.localtest.me
# - Dapr dashboard: daprdashboard.localtest.me
# - Grafana: grafana.localtest.me, Jaeger: jaeger.localtest.me
```

### Building Services
```bash
# .NET services (from src/)
dotnet build src/FastFoodDelivery.sln

# Frontend services (in each clientapp/ dir)
npm install
npm run build  # Vite build
npm run dev    # Development server

# Docker images
docker compose build <service-name>
./BuildAndPushDockerImages.ps1  # Builds all services
```

### Running Tests
```bash
# Unit tests (.NET)
dotnet test <path-to-test-project.csproj>

# Frontend tests
npm test        # Vitest run
npm run test:watch  # Watch mode

# Tests are also embedded in Dockerfiles (multi-stage builds)
# Example: OrderService Dockerfile has "test" stage extracting results to /testresults
```

### Self-Hosted Dapr (Alternative)
Services include `Start-Selfhosted.ps1` scripts for running with Dapr CLI:
```powershell
# Example: src/services/order/OrderService/Start-Selfhosted.ps1
dapr run --app-id orderservice --app-port 8080 --resources-path ../../../infrastructure-dev/dapr/resources ...
```

## Observability Standards

All services use **OpenTelemetry** via `FastFood.Observability.Common`:

**Implementation Pattern:**
```csharp
// Service-specific observability class extends ObservabilityBase
public class OrderServiceObservability : ObservabilityBase, IOrderServiceObservability
{
    public Counter<long> OrdersCreatedCounter { get; }
    public Histogram<double> OrderTotalDuration { get; }
}

// Usage in classes
using var activity = _observability.StartActivity(this.GetType(), includeCallerTypeInName: true);
_observability.OrdersCreatedCounter.Add(1, new KeyValuePair<string, object?>("orderType", order.Type));
```

**Configuration:**
- Set via `Observability__UseTracingExporter=otlp` environment variables
- Exporters: OTLP (default), Console, Zipkin (legacy)
- Collector: OpenTelemetry Collector at `otel-collector:4317`
- Backends: Jaeger (traces), Prometheus (metrics), Loki (logs)

**Key Metrics:**
- `OrdersCreatedCounter`, `OrderTotalDuration` (Order lifecycle)
- Activity sources follow pattern: `{ServiceName}.{ClassName}.{MethodName}`

## CI/CD Pipeline Structure

**Azure DevOps Pipelines** (`pipelines/`):

**CI Pipelines** (`ci-{service}.yml`):
- Trigger on `main`, `release/*` branches + specific path changes
- Use custom container: `fastfood-buildenv` with .NET SDK
- Template: `build/job-buildcontainerizedservice.yml`
- Build Docker images, run tests in containers, extract test results
- Build artifacts: Docker images + Helm charts

**CD Pipelines** (`cd-{service}.yml`):
- Multi-stage: Dev → Test → Prod
- Template: `deploy/job-deployservicetok8s.yml`
- Deploy Helm charts to AKS clusters
- PR-specific deployments include verification job

**Configuration:**
- Variables in `pipelines/config/var-{service}.yml`
- Common: `var-pool.yml`, `var-commonvariables.yml`
- Key variables: `dockerFilePath`, `helmChartSourcePath`, `componentPath`

**Multi-Stage Dockerfiles:**
- Stages: `base`, `build`, `test`, `publish`, `final`
- Test stage extracts results: `LABEL testresults=${BUILDID}`
- Final uses chiseled images: `-noble-chiseled-extra` suffix for minimal attack surface

## Technology Stack

**Backend:**
- .NET 9.0 (SDK 9.0.300+)
- Dapr SDK 1.16.0 (`Dapr.AspNetCore`, `Dapr.Actors`, `Dapr.Workflow`)
- xUnit for testing with Coverlet for coverage

**Frontend:**
- Vue 3.5+ with Composition API
- Vite 5.4+ build tool
- Pinia for state management
- Tailwind CSS 4.1+
- Vitest for testing
- SignalR for real-time updates

**Infrastructure:**
- Docker Compose for local orchestration
- Kubernetes (AKS) for production
- Helm charts in `src/chart/`
- Traefik 3.5 as ingress/proxy

**Dapr Components:**
- Redis 8.2 (state + actor store)
- RabbitMQ 4.1 (pub/sub)
- mTLS enabled with custom CA certs (`infrastructure-dev/dapr/certs/`)

## Common Gotchas

1. **Port Conflicts**: Each service + sidecar uses unique ports. Check docker-compose.yml for mappings.

2. **Dapr Placement**: Required for actors. Must start before actor-using services.

3. **mTLS Certificates**: Services use `../infrastructure-dev/dapr/certs/mtls.env`. Regenerate if expired (see `infrastructure-dev/dapr/certs/README.md`).

4. **Event Routing**: When adding order lifecycle events, update `OrderEventRouter` logic and ensure all three implementation patterns handle it.

5. **Frontend Builds**: ASP.NET Core hosts SPAs. Run `npm run build` in `clientapp/` before running .NET projects in Release mode.

6. **Test Extraction**: CI pipelines expect `/testresults` directory in Docker `test` stage with `testresults` label.

7. **Service Dependencies**: Dapr sidecars defined as separate containers sharing network via `network_mode: "service:{app-name}"`.

8. **Feature Flags**: Feature flags use Microsoft.FeatureManagement. Local dev uses appsettings.json; deployed environments can use Azure App Configuration via connection string or Managed Identity.

## Feature Flag System

The application uses **Microsoft.FeatureManagement** for feature flags with OpenTelemetry integration:

### Architecture
- **Shared Library**: `FastFood.FeatureManagement.Common` provides constants, observability integration, and configuration
- **Local Dev**: Feature flags in `appsettings.json` → `"FeatureManagement": { "FlagName": true }`
- **Production**: Azure App Configuration (optional) with 30s cache refresh
- **Observability**: All evaluations tracked via `FeatureEvaluationCounter` and activity tags (`feature.{name}.enabled`)

### Implemented Feature Flags

| Flag | Services | Purpose |
|------|----------|---------|
| `LoyaltyProgram` | OrderService, FrontendSelfServicePos | 10% discount for loyalty members |
| `NewCheckoutExperience` | FrontendSelfServicePos | A/B test alternative checkout UI |
| `DarkMode` | FrontendSelfServicePos | Dark theme toggle |
| `UseWorkflowImplementation` | OrderService | Route orders to Workflow vs Actor/State |
| `DynamicPricing` | OrderService | 1.2x surge pricing multiplier |
| `AutoPrioritization` | KitchenService | Smart kitchen queue ordering |

### Usage Pattern
```csharp
// Inject IObservableFeatureManager
private readonly IObservableFeatureManager _featureManager;

// Evaluate feature
var enabled = await _featureManager.IsEnabledAsync(FeatureFlags.LoyaltyProgram);

// Record usage
if (enabled)
{
    FeatureFlagActivityEnricher.RecordFeatureUsage(activity, FeatureFlags.LoyaltyProgram, "discount_applied");
    _observability.FeatureUsageCounter.Add(1, 
        new("feature", FeatureFlags.LoyaltyProgram),
        new("action", "discount_applied"));
}
```

### Configuration Examples
**Simple Boolean**: `"LoyaltyProgram": true`  
**Time Window**: 
```json
"DynamicPricing": {
  "EnabledFor": [{
    "Name": "Microsoft.TimeWindow",
    "Parameters": { "Start": "11:00:00", "End": "14:00:00" }
  }]
}
```

See `demos/FeatureFlags.md` for complete demo script and setup instructions.

## Key Files Reference

- `src/common/FastFood.Common/Constants.cs` - Central constants for Dapr component names, event names, service IDs
- `src/common/FastFood.FeatureManagement.Common/` - Feature flag infrastructure and constants
- `infrastructure-dev/dapr/resources/` - Dapr component definitions (pubsub, statestore, secrets)
- `infrastructure-dev/dapr/config/config.yaml` - Dapr configuration (mTLS, tracing)
- `src/docker-compose.yml` - Full local environment
- `pipelines/build/job-buildcontainerizedservice.yml` - Reusable CI build template
- `src/services/order/OrderService/Services/OrderEventRouter.cs` - Order routing logic with feature flag support
- `src/common/FastFood.Observability.Common/` - Shared observability infrastructure
- `demos/FeatureFlags.md` - Complete feature flag demo script

## Naming Conventions

- **Services**: lowercase, no hyphens (e.g., `orderservice`, `kitchenservice`)
- **Dapr App IDs**: Match service names
- **State Keys**: Use descriptive prefixes (`OrderProcessing-`, `WF-Order-`)
- **Events**: Lowercase, no spaces (`ordercreated`, `kitchenitemfinished`)
- **Docker Images**: Match service names in lowercase
- **Helm Charts**: In `src/chart/{service-name}/`


> **Project tools:** `fastfood-mcp` (local MCP).
> **Primary goal:** Answer questions and generate changes **using fastfood-mcp tools** whenever they apply. Prefer tool calls over guessing.

## When to call which tool

* **Errors**

  * `ExplainError(code)` → any mention of an internal error code (e.g., E2145, P5001) or “what does this error mean?”
  * `SearchErrors(query, limit)` → user gives a log line/keyword but not a code, or `ExplainError` returns “not found.”
  * `SuggestFix(code)` → the user asks for concrete remediation steps or a runbook summary.
* **Services (system awareness)**

  * `GetService(name)` → user asks what a service does, repo, language, or its API list.
  * `ListDependencies(name, direction)`

    * `outbound` → “what does X depend on?”
    * `inbound` → “who depends on X?”
  * `FindEndpoint(name, [path])` → user asks about available routes or filters by a path fragment.
  * `ServiceOwner(name)` → user asks for owners, Slack channel, or runbook.
* **Feature flags**

  * `ListFlags([service])` → enumerate flags (optionally scoped to a service).
  * `GetFlag(key)` → full definition and environments.
  * `FlagStatus(key, environment)` → resolve the *effective* value in `dev | staging | prod`.

## Execution rules

1. **Prefer tools** over assumptions for anything about errors, services/dependencies, feature flags.
2. If a lookup fails, **immediately try the fuzzy/backup tool** (e.g., `SearchErrors` after a miss; or suggest top 3 close service names from the response).
3. **Surface links** (runbooks) from tool responses when proposing steps.
4. When writing code/tests that depend on a **feature flag**, call `FlagStatus` first and generate **parameterized tests** or branches for true/false (or multivariants).
5. When planning a change, call `ListDependencies` (both directions if risk is discussed) and name owners via `ServiceOwner` for review routing.
6. Keep answers **actionable**: summarize tool result → concrete next steps → (optionally) code or commands.