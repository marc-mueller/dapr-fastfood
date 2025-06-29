# Hands-On Lab: Building and Deploying a Service with Azure Pipelines

## Overview

In this lab, you will:
- Learn the structure and purpose of a CI/CD pipeline for the `frontendselfservicepos` service.
- Understand each part of the pipeline YAML files, including triggers, variables, jobs, and deployment steps.
- Build your own CI and CD pipelines step by step, with explanations for each section.
- At the end, see the complete pipeline YAML for reference.

**Goal:** By the end of this lab, you will understand how to author and reason about a working CI/CD pipeline for one service using Azure Pipelines.

---

## Prerequisites

- Access to the FastFoodDelivery repository (with all folders except `pipelines/`).
- Azure DevOps project with permissions to create pipelines and service connections.
- An Azure Container Registry (ACR) and a Kubernetes cluster (AKS) with service connections set up.
- The following variable templates are available and can be used:
  - `config/var-pool.yml`
  - `config/var-commonvariables.yml`
  - `config/var-frontendselfservicepos.yml`

---

## Step 1: Understanding the CI Pipeline

The CI (Continuous Integration) pipeline is responsible for building your application, running tests, and publishing build artifacts (such as Docker images and Helm charts). Let's break down each part of the pipeline.

### 1.1. Pipeline Trigger

```yaml
trigger:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - src/services/frontendselfservicepos/**
```
**Explanation:**
- The pipeline will run automatically when changes are pushed to the `main` branch or any `release/*` branch.
- It will only trigger if the changes are inside the `src/services/frontendselfservicepos/` folder. This helps avoid unnecessary builds for unrelated changes.

### 1.2. Variables

```yaml
variables:
  - template: config/var-pool.yml
  - template: config/var-commonvariables.yml
  - template: config/var-frontendselfservicepos.yml
```
**Explanation:**
- Variables are imported from shared YAML files. These files contain values like image names, registry URLs, and other settings that are reused across pipelines.
- This makes your pipeline easier to maintain and update.

### 1.3. Pool

```yaml
pool:
  vmImage: 'ubuntu-latest'
```
**Explanation:**
- The pipeline will run on a Microsoft-hosted Ubuntu agent. This agent comes with all the tools needed for building .NET, Docker, and Helm projects.
- For now, we are **not** using custom containers for the build agent.

### 1.4. Jobs and Steps

```yaml
jobs:
  - job: build
    displayName: "Build and Publish Frontend Self Service POS"
    steps:
      - checkout: self
        fetchDepth: 0
        persistCredentials: true
        clean: true
        lfs: true
```
**Explanation:**
- A job is a collection of steps that run on the same agent.
- The first step checks out your code from the repository. The options ensure you get the full history, clean workspace, and support for large files (LFS).

#### Building the Docker Image

```yaml
      - task: Docker@2
        displayName: Build Docker image
        inputs:
          command: build
          Dockerfile: $(dockerFilePath)
          repository: $(dockerRepositoryName)
          tags: |
            $(Build.BuildId)
          buildContext: $(Build.SourcesDirectory)/src
          arguments: --build-arg IMAGE_NET_ASPNET_VERSION=$(netCoreAspNetVersion) --build-arg IMAGE_NET_SDK_VERSION=$(netCoreSdkVersion)
```
**Explanation:**
- This step builds a Docker image for the service using the specified Dockerfile and build context.
- The image is tagged with the build ID for traceability.
- Build arguments are passed in to control the .NET versions used in the image.

#### Pushing the Docker Image

```yaml
      - task: Docker@2
        displayName: Push Docker image
        inputs:
          command: push
          repository: $(dockerRepositoryName)
          tags: |
            $(Build.BuildId)
```
**Explanation:**
- This step pushes the built Docker image to your Azure Container Registry.
- The tag matches the one used in the build step.

#### Publishing the Helm Chart

```yaml
      - task: PublishPipelineArtifact@1
        displayName: Publish Helm chart
        inputs:
          targetPath: $(helmChartSourcePath)
          artifact: $(pipelineArtifactName)
          publishLocation: pipeline
```
**Explanation:**
- This step publishes the Helm chart for your service as a pipeline artifact.
- The CD pipeline will later download this artifact for deployment.

---

## Step 2: Understanding the CD Pipeline

The CD (Continuous Deployment) pipeline is responsible for deploying your application to a Kubernetes cluster. Let's break down each part.

### 2.1. Pipeline Trigger

```yaml
trigger: none
```
**Explanation:**
- This pipeline does not run automatically on code changes. Instead, it is triggered by the CI pipeline via pipeline resources.

### 2.2. Pipeline Resources

```yaml
resources:
  pipelines:
    - pipeline: CIBuild
      source: ci-frontendselfservicepos
      trigger:
        branches:
          include:
            - main
            - release/*
```
**Explanation:**
- This section tells Azure Pipelines to trigger the CD pipeline when the CI pipeline (`ci-frontendselfservicepos`) completes successfully on the specified branches.
- It also makes the CI pipeline's artifacts available for download in the CD pipeline.

### 2.3. Variables and Pool

```yaml
variables:
  - template: config/var-pool.yml
  - template: config/var-commonvariables.yml
  - template: config/var-commonvariables-release.yml
  - template: config/var-frontendselfservicepos.yml

pool:
  vmImage: 'ubuntu-latest'
```
**Explanation:**
- The same variable templates are used as in the CI pipeline, plus a release-specific one.
- The pool is again set to use a Microsoft-hosted Ubuntu agent.

### 2.4. Stages, Jobs, and Steps

```yaml
stages:
  - stage: Staging
    displayName: Deploy to Staging
    condition: and(succeeded(), eq(variables['Build.SourceBranchName'], 'main'))
    variables:
      - name: stagename
        value: staging
      - name: namespace
        value: $(stagename)
    jobs:
      - deployment: deploy
        displayName: Deploy Frontend Self Service POS to Staging
        environment: 'fastfood-$(stagename)'
        pool:
          vmImage: 'ubuntu-latest'
        strategy:
          runOnce:
            deploy:
              steps:
                - checkout: self
                  fetchDepth: 1
                - download: CIBuild
                  artifact: $(pipelineArtifactName)
                - task: HelmInstaller@1
                  displayName: Install Helm
                  inputs:
                    helmVersionToInstall: $(helmVersion)
                - task: HelmDeploy@0
                  displayName: Deploy Helm chart
                  inputs:
                    connectionType: 'Kubernetes Service Connection'
                    kubernetesServiceConnection: $(kubernetesDeploymentServiceConnection)
                    namespace: $(namespace)
                    command: upgrade
                    chartType: FilePath
                    chartPath: $(helmChartArtifactDownloadPath)
                    releaseName: $(pipelineArtifactName)
                    overrideValues: |
                      image.repository=$(dockerRepositoryName)
                      image.tag=$(Build.BuildId)
                    valueFile: $(helmValuesOverwritesFile)
```
**Explanation:**
- The pipeline has a single stage called `Staging`.
- The deployment job downloads the Helm chart artifact, installs Helm, and deploys the service to the Kubernetes cluster using the specified service connection and namespace.
- The `overrideValues` section ensures the correct image and tag are used for the deployment.

---

## Step 3: Putting It All Together

Now that you understand each part, you can assemble the full pipeline files. Below are the complete YAML files for both CI and CD pipelines. You can use these as a reference or starting point for your own implementation.

### 3.1. Complete CI Pipeline

```yaml
name: ci-frontendselfservicepos

trigger:
  branches:
    include:
      - main
      - release/*
  paths:
    include:
      - src/services/frontendselfservicepos/**

variables:
  - template: config/var-pool.yml
  - template: config/var-commonvariables.yml
  - template: config/var-frontendselfservicepos.yml

pool:
  vmImage: 'ubuntu-latest'

jobs:
  - job: build
    displayName: "Build and Publish Frontend Self Service POS"
    steps:
      - checkout: self
        fetchDepth: 0
        persistCredentials: true
        clean: true
        lfs: true
      - task: Docker@2
        displayName: Build Docker image
        inputs:
          command: build
          Dockerfile: $(dockerFilePath)
          repository: $(dockerRepositoryName)
          tags: |
            $(Build.BuildId)
          buildContext: $(Build.SourcesDirectory)/src
          arguments: --build-arg IMAGE_NET_ASPNET_VERSION=$(netCoreAspNetVersion) --build-arg IMAGE_NET_SDK_VERSION=$(netCoreSdkVersion)
      - task: Docker@2
        displayName: Push Docker image
        inputs:
          command: push
          repository: $(dockerRepositoryName)
          tags: |
            $(Build.BuildId)
      - task: PublishPipelineArtifact@1
        displayName: Publish Helm chart
        inputs:
          targetPath: $(helmChartSourcePath)
          artifact: $(pipelineArtifactName)
          publishLocation: pipeline
```

### 3.2. Complete CD Pipeline

```yaml
name: cd-frontendselfservicepos

trigger: none

resources:
  pipelines:
    - pipeline: CIBuild
      source: ci-frontendselfservicepos
      trigger:
        branches:
          include:
            - main
            - release/*

pool:
  vmImage: 'ubuntu-latest'

variables:
  - template: config/var-pool.yml
  - template: config/var-commonvariables.yml
  - template: config/var-commonvariables-release.yml
  - template: config/var-frontendselfservicepos.yml

stages:
  - stage: Staging
    displayName: Deploy to Staging
    condition: and(succeeded(), eq(variables['Build.SourceBranchName'], 'main'))
    variables:
      - name: stagename
        value: staging
      - name: namespace
        value: $(stagename)
    jobs:
      - deployment: deploy
        displayName: Deploy Frontend Self Service POS to Staging
        environment: 'fastfood-$(stagename)'
        pool:
          vmImage: 'ubuntu-latest'
        strategy:
          runOnce:
            deploy:
              steps:
                - checkout: self
                  fetchDepth: 1
                - download: CIBuild
                  artifact: $(pipelineArtifactName)
                - task: HelmInstaller@1
                  displayName: Install Helm
                  inputs:
                    helmVersionToInstall: $(helmVersion)
                - task: HelmDeploy@0
                  displayName: Deploy Helm chart
                  inputs:
                    connectionType: 'Kubernetes Service Connection'
                    kubernetesServiceConnection: $(kubernetesDeploymentServiceConnection)
                    namespace: $(namespace)
                    command: upgrade
                    chartType: FilePath
                    chartPath: $(helmChartArtifactDownloadPath)
                    releaseName: $(pipelineArtifactName)
                    overrideValues: |
                      image.repository=$(dockerRepositoryName)
                      image.tag=$(Build.BuildId)
                    valueFile: $(helmValuesOverwritesFile)
```

---

## Step 4: Commit and Push

1. Add both pipeline files to your repository:
   ```sh
   git add .azure-pipelines/ci-frontendselfservicepos.yml .azure-pipelines/cd-frontendselfservicepos.yml
   git commit -m "Add CI/CD pipelines for frontendselfservicepos (HOL step 1)"
   git push
   ```

2. Create both pipelines in Azure DevOps, pointing to the respective YAML files.

---

## Step 5: Run the Pipelines

- Make a change in `src/services/frontendselfservicepos/` and push to `main` or `release/*`.
- The CI pipeline will build and publish the image and Helm chart, then trigger the CD pipeline.
- The CD pipeline will deploy the service to the Staging namespace in your AKS cluster.

---

## Summary

You have now:
- Learned the structure and purpose of each part of a CI/CD pipeline.
- Authored a CI pipeline to build and publish a container image and Helm chart.
- Authored a CD pipeline to deploy the service to Kubernetes.
- Used only inlined YAML (no templates) and variable templates for configuration.

**Next steps:** In future labs, you will refactor these pipelines to use templates, add more stages, and handle additional services.

---

If you need help with service connections, Helm, or Azure DevOps setup, refer to the official documentation or ask your instructor.
