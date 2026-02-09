---
applyTo: "**/Dockerfile"
---

# Dockerfile Guidelines

## Multi-Stage Build Pattern

All service Dockerfiles follow this structure:

```dockerfile
# Stage 1: Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS base
WORKDIR /app
EXPOSE 8080

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILDID=local
WORKDIR /src
COPY ["src/services/order/OrderService/OrderService.csproj", "src/services/order/OrderService/"]
COPY ["src/common/FastFood.Common/FastFood.Common.csproj", "src/common/FastFood.Common/"]
# ... other project references
RUN dotnet restore "src/services/order/OrderService/OrderService.csproj"
COPY . .
RUN dotnet build "src/services/order/OrderService/OrderService.csproj" -c Release -o /app/build

# Stage 3: Test (for CI extraction)
FROM build AS test
LABEL testresults=${BUILDID}
WORKDIR /src
RUN dotnet test --logger "trx;LogFileName=test-results.trx" --results-directory /testresults

# Stage 4: Publish
FROM build AS publish
RUN dotnet publish "src/services/order/OrderService/OrderService.csproj" -c Release -o /app/publish

# Stage 5: Final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderService.dll"]
```

## Key Requirements

**Test stage label**: CI pipelines extract test results using:
```dockerfile
LABEL testresults=${BUILDID}
```

**Chiseled images**: Use `-noble-chiseled-extra` suffix for minimal attack surface:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0-noble-chiseled-extra AS base
```

**Build context**: Dockerfiles expect build context at repo root to access:
- `src/common/` shared libraries
- `src/services/{service}/` service projects

## Frontend Services (with Vue.js)

Frontend Dockerfiles include npm build step:

```dockerfile
# Build Vue.js SPA
FROM node:22-alpine AS clientbuild
WORKDIR /clientapp
COPY src/services/frontendselfservicepos/FrontendSelfServicePos/clientapp/package*.json ./
RUN npm ci
COPY src/services/frontendselfservicepos/FrontendSelfServicePos/clientapp/ .
RUN npm run build

# Copy built SPA to ASP.NET wwwroot
FROM build AS publish
COPY --from=clientbuild /clientapp/dist ./wwwroot
RUN dotnet publish -c Release -o /app/publish
```

## Build Commands

```bash
# Single service (from repo root)
docker build -f src/services/order/OrderService/Dockerfile -t orderservice .

# All services via compose
cd src && docker compose build

# With build ID for CI
docker build --build-arg BUILDID=$(Build.BuildId) -f Dockerfile -t service:$(Build.BuildId) .
```
