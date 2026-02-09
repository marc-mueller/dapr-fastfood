# FastFood Delivery - AI Coding Agent Instructions

## System Overview

**Microservices fast food delivery system** using Dapr for service communication, state, pub/sub, and actors.

### Services
| Service | Path | Purpose |
|---------|------|---------|
| OrderService | `src/services/order/` | Order management (3 implementation patterns) |
| OrderService.Actors | `src/services/order/OrderService.Actors/` | Dapr actors host |
| KitchenService | `src/services/kitchen/` | Kitchen processing |
| FinanceService | `src/services/finance/` | Payment operations |
| FrontendSelfServicePos | `src/services/frontendselfservicepos/` | POS Vue.js SPA |
| FrontendKitchenMonitor | `src/services/frontendkitchenmonitor/` | Kitchen display |
| FrontendCustomerOrderStatus | `src/services/frontendcustomerorderstatus/` | Order tracking |

## Architecture

### Triple Implementation (OrderService)
Orders use ONE of three patterns (demo showcase):
1. **Actor-based** (`OrderProcessingServiceActor`) - Dapr Actors
2. **State-based** (`OrderProcessingServiceState`) - Direct state + pub/sub
3. **Workflow-based** (`OrderProcessingServiceWorkflow`) - Dapr Workflows

Routing via `OrderEventRouter` stores target in Redis (`OrderEventRouterTarget-{orderId}`).

### Dapr Components
- **Pub/Sub**: `pubsub` (RabbitMQ) - events in `FastFoodConstants.EventNames`
- **State**: `statestore` (Redis) - keys: `OrderProcessing-{id}`, `WF-Order-{id}`
- **Actors**: Separate service, create via `ActorProxy.Create<IOrderActor>()`

## Development

```bash
# Infrastructure only
cd infrastructure-dev && ./StartInfrastructureServices.ps1

# Full stack
cd src && docker compose up

# Build
dotnet build src/FastFoodDelivery.sln
cd src/services/*/clientapp && npm install && npm run build

# Tests
dotnet test <project.csproj>
npm test  # in clientapp/
```

**Access**: `pos.localtest.me`, `kitchen.localtest.me`, `daprdashboard.localtest.me`, `grafana.localtest.me`

## Key Files

- `src/common/FastFood.Common/Constants.cs` - Dapr names, events, services
- `src/common/FastFood.FeatureManagement.Common/` - Feature flag infrastructure
- `src/common/FastFood.Observability.Common/` - OpenTelemetry patterns
- `infrastructure-dev/dapr/resources/` - Dapr component definitions

## Feature Flags

| Flag | Services | Purpose |
|------|----------|---------|
| `LoyaltyProgram` | OrderService, FrontendSelfServicePos | 10% loyalty discount |
| `UseWorkflowImplementation` | OrderService | Route to Workflow pattern |
| `DynamicPricing` | OrderService | Surge pricing |
| `AutoPrioritization` | KitchenService | Smart queue ordering |

## Technology

- .NET 10.0, Dapr 1.16.0, xUnit
- Vue 3.5+, Vite 7.2+, Pinia, Tailwind CSS 4.1+, Vitest 4.0+
- Docker Compose / Kubernetes (AKS), Helm, Traefik

---

> **Project tools**: `fastfood-mcp` (local MCP)

## MCP Tool Dispatch

**Errors**: `ExplainError(code)` -> `SearchErrors(query)` -> `SuggestFix(code)`

**Services**: `GetService(name)`, `ListDependencies(name, direction)`, `FindEndpoint(name)`, `ServiceOwner(name)`

**Feature flags**: `ListFlags([service])`, `GetFlag(key)`, `FlagStatus(key, environment)`

**Rules**:
1. Prefer tools over assumptions for errors, services, flags
2. On lookup miss, try fuzzy backup tool
3. Surface runbook links from responses
4. For flag-dependent code, call `FlagStatus` first and generate branches for both states
