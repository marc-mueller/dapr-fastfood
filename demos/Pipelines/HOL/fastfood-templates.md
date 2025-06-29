# Hands-On Lab: Using Templates for Scalable CI/CD Pipelines in Azure DevOps

## Overview

In this lab, you will:
- Learn why code duplication in pipelines is a problem when managing multiple services.
- Understand how Azure Pipelines templates help you avoid duplication and improve maintainability.
- Explore step templates and job templates for both build and deployment.
- See how to use these templates to create concise, maintainable CI and CD pipelines for all your services.
- At the end, review what the final pipeline and template structure should look like, with concrete examples.

**Goal:** By the end of this lab, you will know how to use templates to build scalable, DRY (Don't Repeat Yourself) CI/CD pipelines for many services.

---

## The Challenge: Many Services, Much Duplication

As your solution grows, you will have many microservices (e.g., OrderService, KitchenService, FrontendSelfServicePOS, etc.). Each service needs to be built, containerized, and deployed in a similar way. If you copy-paste the same YAML steps into every pipeline, you will:
- Have a lot of duplicated code.
- Make maintenance difficult (a change in one place must be copied everywhere).
- Increase the risk of errors and inconsistencies.

**Solution:** Use Azure Pipelines templates to define reusable steps and jobs.

---

## Step 1: Creating Step Templates for Build

### 1.1. Step Template: Build and Publish Docker Image

File: `pipelines/build/step-buildandpublishdockerimage.yml`

This template encapsulates all the logic for building and publishing a Docker image, including handling build arguments, secrets, and publishing test results or artifacts.

**Key Parameters:**
- `dockerFile`: Path to the Dockerfile for the service.
- `dockerRepositoryName`: Name of the Docker repository in your ACR.
- `buildContext`: Path to the build context (usually the service source directory).
- `azureContainerRegistryServiceConnection`: Name of the Azure DevOps service connection for ACR.
- `buildId`, `buildNumber`: Build identifiers for tagging images.
- `netCoreAspNetVersion`, `netCoreSdkVersion`: .NET versions for build args.
- `nugetFeeds`, `publishArtifacts`: Optional, for advanced scenarios.

**Example usage:**
```yaml
- template: build/step-buildandpublishdockerimage.yml
  parameters:
    dockerFile: '$(dockerFilePath)'
    dockerRepositoryName: '$(dockerRepositoryName)'
    buildContext: '$(Build.SourcesDirectory)/src'
    azureContainerRegistryServiceConnection: '$(azureContainerRegistryServiceConnection)'
    buildId: '$(Build.BuildId)'
    buildNumber: '$(Build.BuildNumber)'
    netCoreAspNetVersion: '$(netCoreAspNetVersion)'
    netCoreSdkVersion: '$(netCoreSdkVersion)'
    nugetFeeds: []
    publishArtifacts: []
```

**What to do:**
- For each service, set the correct Dockerfile path, repository name, and build context.
- Use the provided variable templates for the other parameters.

### 1.2. Step Template: Build and Publish Helm Package

File: `pipelines/build/step-buildhelmpackage.yml`

This template packages a Helm chart and publishes it as a pipeline artifact.

**Key Parameters:**
- `chartPath`: Path to the Helm chart for the service.
- `artifactName`: Name for the published artifact.
- `artifactStagingDirectory`: Directory to stage the artifact.
- `helmVersion`: Helm version to use (from variables).

**Example usage:**
```yaml
- template: build/step-buildhelmpackage.yml
  parameters:
    chartPath: '$(helmChartSourcePath)'
    artifactName: '$(pipelineArtifactName)'
    artifactStagingDirectory: '$(Build.ArtifactStagingDirectory)'
    helmVersion: '$(helmVersion)'
```

**What to do:**
- Set the correct chart path and artifact name for each service.
- Use the variable templates for the other parameters.

---

## Step 2: Creating a Job Template for Build

### 2.1. Job Template: Build and Publish Containerized Service

File: `pipelines/build/job-buildcontainerizedservice.yml`

This job template brings together the step templates above and defines the full build job for a service.

**Key Parameters:**
- `dockerFile`, `dockerRepositoryName`, `chartPath`, `artifactName`, `azureContainerRegistryServiceConnection`, `netCoreAspNetVersion`, `netCoreSdkVersion`, `helmVersion`, `artifactStagingDirectory`, `buildContext`, `buildId`, `buildNumber`, `servicePaths`, `serviceTagPrefix`, `installMonotag`, `updateBuildNumber`, `nugetFeeds`, `publishArtifacts`, `dockerArguments`, `pool`, `container`.

**Example usage in a CI pipeline:**
```yaml
jobs:
  - template: build/job-buildcontainerizedservice.yml
    parameters:
      displayName: "Build and Publish Order Service"
      dockerFile: '$(dockerFilePath)'
      dockerRepositoryName: '$(dockerRepositoryName)'
      chartPath: '$(helmChartSourcePath)'
      artifactName: '$(pipelineArtifactName)'
      azureContainerRegistryServiceConnection: '$(azureContainerRegistryServiceConnection)'
      netCoreAspNetVersion: '$(netCoreAspNetVersion)'
      netCoreSdkVersion: '$(netCoreSdkVersion)'
      helmVersion: '$(helmVersion)'
      artifactStagingDirectory: '$(Build.ArtifactStagingDirectory)'
      buildContext: '$(Build.SourcesDirectory)/src'
      buildId: '$(Build.BuildId)'
      buildNumber: '$(Build.BuildNumber)'
      servicePaths:
        - '$(componentPath)'
      serviceTagPrefix: '$(versionTagPrefix)'
      installMonotag: false
      updateBuildNumber: true
      nugetFeeds: []
      publishArtifacts: []
      dockerArguments: '--build-arg IMAGE_NET_ASPNET_VERSION=$(netCoreAspNetVersion) --build-arg IMAGE_NET_SDK_VERSION=$(netCoreSdkVersion)'
      pool:
        vmImage: 'ubuntu-latest'
      container: ''
```

**What to do:**
- For each service, use the correct variable templates for all parameters.
- Adjust `servicePaths`, `serviceTagPrefix`, and `dockerArguments` as needed for your service.

---

## Step 3: Building CI Pipelines Using the Job Template

Now, each CI pipeline for a service (e.g., `ci-orderservice.yml`, `ci-kitchenservice.yml`, etc.) can be very concise and maintainable. Here is a complete example for the OrderService:

```yaml
name: ci-orderservice

trigger:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - src/services/orderservice/**

variables:
  - template: config/var-pool.yml
  - template: config/var-commonvariables.yml
  - template: config/var-orderservice.yml

jobs:
  - template: build/job-buildcontainerizedservice.yml
    parameters:
      displayName: "Build and Publish Order Service"
      dockerFile: '$(dockerFilePath)'
      dockerRepositoryName: '$(dockerRepositoryName)'
      chartPath: '$(helmChartSourcePath)'
      artifactName: '$(pipelineArtifactName)'
      azureContainerRegistryServiceConnection: '$(azureContainerRegistryServiceConnection)'
      netCoreAspNetVersion: '$(netCoreAspNetVersion)'
      netCoreSdkVersion: '$(netCoreSdkVersion)'
      helmVersion: '$(helmVersion)'
      artifactStagingDirectory: '$(Build.ArtifactStagingDirectory)'
      buildContext: '$(Build.SourcesDirectory)/src'
      buildId: '$(Build.BuildId)'
      buildNumber: '$(Build.BuildNumber)'
      servicePaths:
        - '$(componentPath)'
      serviceTagPrefix: '$(versionTagPrefix)'
      installMonotag: false
      updateBuildNumber: true
      nugetFeeds: []
      publishArtifacts: []
      dockerArguments: '--build-arg IMAGE_NET_ASPNET_VERSION=$(netCoreAspNetVersion) --build-arg IMAGE_NET_SDK_VERSION=$(netCoreSdkVersion)'
      pool:
        vmImage: 'ubuntu-latest'
      container: ''
```

**What to do:**
- Repeat this pattern for each service, changing only the variable template and trigger path.

---

## Step 4: Creating Step and Job Templates for Deployment

### 4.1. Step Template: Deploy Helm Chart

File: `pipelines/deploy/step-deployhelmchart.yml`

This template encapsulates the logic for deploying a Helm chart to Kubernetes, including installing Helm/Kubectl, creating namespaces, and handling secrets.

**Key Parameters:**
- `kubernetesDeploymentServiceConnection`: Name of the Kubernetes service connection.
- `clusterNamespace`: Namespace to deploy to.
- `chartPath`: Path to the Helm chart package.
- `releaseName`: Name of the Helm release.
- `releaseValuesFile`: Path to the values file.
- `tokenizerSecrets`: Optional, for secret replacement.
- `installHelm`, `installKubectl`, `helmVersion`, `kubectlVersion`, `createNamespace`, `deployOnlyIfNotExist`: Advanced options.

**Example usage:**
```yaml
- template: deploy/step-deployhelmchart.yml
  parameters:
    kubernetesDeploymentServiceConnection: '$(kubernetesDeploymentServiceConnection)'
    clusterNamespace: '$(namespace)'
    chartPath: '$(helmChartArtifactDownloadPath)'
    releaseName: '$(pipelineArtifactName)'
    releaseValuesFile: '$(helmValuesOverwritesFile)'
    tokenizerSecrets: []
    installHelm: true
    installKubectl: true
    helmVersion: '$(helmVersion)'
    kubectlVersion: 'latest'
    createNamespace: false
    deployOnlyIfNotExist: false
```

**What to do:**
- Use the correct variable templates for each parameter.
- Adjust advanced options only if needed.

### 4.2. Job Template: Deploy Service to Kubernetes

File: `pipelines/deploy/job-deployservicetok8s.yml`

This job template defines a deployment job that uses the step template above.

**Key Parameters:**
- `environment`, `namespace`, `valuesFile`, `artifactName`, `chartPackage`, `kubernetesDeploymentServiceConnection`, `updateBuildNumber`, `sparseCheckoutDirectories`, `tokenizerSecrets`, `displayName`, `pool`, `container`.

**Example usage in a CD pipeline:**
```yaml
jobs:
  - template: deploy/job-deployservicetok8s.yml
    parameters:
      environment: 'fastfood-staging'
      namespace: 'staging'
      valuesFile: '$(helmValuesOverwritesFile)'
      artifactName: '$(pipelineArtifactName)'
      chartPackage: '$(helmChartArtifactDownloadPath)'
      kubernetesDeploymentServiceConnection: '$(kubernetesDeploymentServiceConnection)'
      updateBuildNumber: true
      sparseCheckoutDirectories: '$(releaseSparseCheckoutDirectories)'
      tokenizerSecrets: []
      displayName: 'Deploy Service to Kubernetes'
      pool:
        vmImage: 'ubuntu-latest'
      container: ''
```

**What to do:**
- Use the correct variable templates for each parameter.
- Set `updateBuildNumber` and `sparseCheckoutDirectories` as needed for your environment.

---

## Step 5: Building CD Pipelines Using the Job Template

Each CD pipeline for a service (e.g., `cd-orderservice.yml`, `cd-kitchenservice.yml`, etc.) can now be concise and maintainable. Here is a complete example for the OrderService:

```yaml
name: cd-orderservice

trigger: none

resources:
  pipelines:
    - pipeline: CIBuild
      source: ci-orderservice
      trigger:
        branches:
          include:
            - main
            - release/*

variables:
  - template: config/var-pool.yml
  - template: config/var-commonvariables.yml
  - template: config/var-commonvariables-release.yml
  - template: config/var-orderservice.yml

stages:
  - stage: Staging
    jobs:
      - template: deploy/job-deployservicetok8s.yml
        parameters:
          environment: 'fastfood-staging'
          namespace: 'staging'
          valuesFile: '$(helmValuesOverwritesFile)'
          artifactName: '$(pipelineArtifactName)'
          chartPackage: '$(helmChartArtifactDownloadPath)'
          kubernetesDeploymentServiceConnection: '$(kubernetesDeploymentServiceConnection)'
          updateBuildNumber: true
          sparseCheckoutDirectories: '$(releaseSparseCheckoutDirectories)'
          tokenizerSecrets: []
          displayName: 'Deploy Service to Kubernetes'
          pool:
            vmImage: 'ubuntu-latest'
          container: ''
```

**What to do:**
- Repeat this pattern for each service, changing only the variable template and resource pipeline name.

---

## Step 6: Summary and Final Structure

By using step and job templates, you:
- Avoid code duplication across pipelines for all your services.
- Make it easy to update build or deployment logic in one place.
- Ensure consistency and reduce errors.

**Your final structure will look like:**

```
pipelines/
  build/
    step-buildandpublishdockerimage.yml
    step-buildhelmpackage.yml
    job-buildcontainerizedservice.yml
  deploy/
    step-deployhelmchart.yml
    job-deployservicetok8s.yml
  ci-orderservice.yml
  ci-kitchenservice.yml
  ci-frontendselfservicepos.yml
  ci-frontendkitchenmonitor.yml
  ci-frontendcustomerorderstatus.yml
  ci-financeservice.yml
  cd-orderservice.yml
  cd-kitchenservice.yml
  cd-frontendselfservicepos.yml
  cd-frontendkitchenmonitor.yml
  cd-frontendcustomerorderstatus.yml
  cd-financeservice.yml
```

Each CI and CD pipeline is short and readable, and all the logic is centralized in templates.

---

If you need help with templates or YAML syntax, refer to the official Azure Pipelines documentation or ask your instructor.
