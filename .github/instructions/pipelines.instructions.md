---
applyTo: "pipelines/**/*.yml"
---

# Azure DevOps Pipeline Guidelines

## Pipeline Structure

**CI pipelines** (`ci-{service}.yml`):
- Trigger on `main`, `release/*` branches + path filters
- Build Docker images, run tests, extract results
- Publish Docker images + Helm charts as artifacts

**CD pipelines** (`cd-{service}.yml`):
- Multi-stage: Dev → Test → Prod
- Deploy Helm charts to AKS
- PR deployments include verification jobs

## Reusable Templates

**Build**: `build/job-buildcontainerizedservice.yml`
```yaml
- template: build/job-buildcontainerizedservice.yml
  parameters:
    dockerFilePath: $(dockerFilePath)
    helmChartSourcePath: $(helmChartSourcePath)
    componentPath: $(componentPath)
```

**Deploy**: `deploy/job-deployservicetok8s.yml`
```yaml
- template: deploy/job-deployservicetok8s.yml
  parameters:
    environment: 'dev'
    helmReleaseName: $(serviceName)
```

## Configuration Variables

Service-specific: `config/var-{service}.yml`
```yaml
variables:
  serviceName: orderservice
  dockerFilePath: src/services/order/OrderService/Dockerfile
  helmChartSourcePath: src/chart/orderservice
  componentPath: src/services/order
```

Common: `config/var-pool.yml`, `config/var-commonvariables.yml`

## Trigger Patterns

```yaml
trigger:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - src/services/order/**
      - src/common/**
      - pipelines/ci-orderservice.yml
```

## Build Container

Builds run in custom container with .NET SDK:
```yaml
container: fastfood-buildenv
```

## Test Result Extraction

CI expects tests to run in Docker `test` stage with results at `/testresults`:
```yaml
- task: Docker@2
  inputs:
    command: build
    arguments: --target test --output type=local,dest=./testresults
```

## Artifact Publishing

```yaml
# Docker image
- task: Docker@2
  inputs:
    command: push
    containerRegistry: $(containerRegistry)

# Helm chart
- task: PublishPipelineArtifact@1
  inputs:
    targetPath: $(helmChartSourcePath)
    artifact: helm-chart
```
