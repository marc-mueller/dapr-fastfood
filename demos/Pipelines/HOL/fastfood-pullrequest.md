# Hands-On Lab: Pull Request Environments, Status Checks, and Cleanup in Azure Pipelines

## Overview

In this lab, you will:
- Learn how PR (Pull Request) environments are handled in the deployment pipelines.
- Understand the dependencies between Staging, Production, and PullRequest stages in your CD pipelines.
- See how to add custom PR status checks using a step template.
- Learn how to automatically clean up PR environments using webhook triggers and a dedicated cleanup pipeline.
- Get full code examples for all templates and pipelines involved.

**Goal:** By the end of this lab, you will know how to implement robust PR deployment, verification, and cleanup in Azure Pipelines.

---

## Step 1: Understanding Staging, Production, and PullRequest Stages

Every CD pipeline (e.g., `cd-frontendselfservicepos.yml`) is structured with multiple stages:
- **Staging:** Deploys to a shared staging environment, usually only for the `main` branch.
- **Production:** Deploys to production, but only after staging has succeeded. This stage depends on Staging.
- **PullRequest:** Deploys a temporary environment for each PR, using a unique namespace. This stage is triggered only for PR builds and is independent of Staging/Production.

**Example from `cd-frontendselfservicepos.yml`:**
```yaml
stages:
  - stage: Staging
    condition: and(succeeded(), eq(variables['Build.SourceBranchName'], 'main'))
    # ...
  - stage: Production
    dependsOn: Staging
    condition: and(succeeded(), eq(variables['Build.SourceBranchName'], 'main'))
    # ...
  - stage: PullRequest
    dependsOn: []
    condition: and(succeeded(), ne(variables['System.PullRequest.PullRequestId'], ''))
    # ...
```
**Explanation:**
- **Staging** and **Production** only run for the `main` branch, and Production only runs if Staging succeeds.
- **PullRequest** runs only for PR builds (when `System.PullRequest.PullRequestId` is set) and is independent of the other stages.

---

## Step 2: Deploying and Testing PR Environments

In the PullRequest stage, a unique namespace is created for each PR (e.g., `pr-123`). The deployment job uses the same deployment template as Staging/Production, but with PR-specific variables.

**Example PR stage in a CD pipeline:**
```yaml
- stage: PullRequest
  dependsOn: []
  condition: and(succeeded(), ne(variables['System.PullRequest.PullRequestId'], ''))
  variables:
    - name: stagename
      value: pr
    - name: namespace
      value: $(stagename)-$(System.PullRequest.PullRequestId)
    - name: redisDB
      value: $[counter(format('{0:yyyyMM}', pipeline.startTime), 3)]
  jobs:
    - template: deploy/job-deployservicetok8s.yml
      parameters:
        environment: 'fastfood-$(stagename)'
        namespace: '$(namespace)'
        valuesFile: '$(helmChartArtifactValuesFileDownloadPath)'
        artifactName: '$(pipelineArtifactName)'
        chartPackage: '$(helmChartArtifactDownloadPath)'
        kubernetesDeploymentServiceConnection: '$(kubernetesDeploymentServiceConnection)'
        updateBuildNumber: true
        
        pool:
          vmImage: 'ubuntu-latest'
    - template: deploy/job-verifyprdeployment.yml
      parameters:
        serviceName: '$(serviceName)'
        displayName: 'Run System Tests for PR $(System.PullRequest.PullRequestId)'
        pool:
          vmImage: 'ubuntu-latest'
```
**Explanation:**
- The deployment job creates a PR-specific environment.
- The verification job runs system tests or other checks against the deployed PR environment.

---

## Step 3: Adding Custom PR Status Checks

You can add custom status checks to your PRs using a step template. This allows you to report the status of deployments, tests, or any other checks directly to the PR in Azure DevOps.

### 3.1. Step Template: Set PR Status

File: `pipelines/pullrequest/step-setprstatus.yml`

**Purpose:**
- Posts a status (e.g., success, failure, pending) to the PR using the Azure DevOps REST API.
- Can be used after deployment, tests, or any custom logic.

**Parameters:**
- `contextName`: Name of the status context (e.g., "PR Deployment").
- `state`: State to set (`pending`, `succeeded`, `failed`).
- `description`: Description for the status.
- `targetUrl`: (Optional) Link to build or test results.
- `genre`, `iterationId`: (Optional) Advanced context.

**Example usage:**
```yaml
- template: pullrequest/step-setprstatus.yml
  parameters:
    contextName: 'PR Deployment'
    state: 'succeeded'
    description: 'PR environment deployed and tested successfully.'
    targetUrl: '$(System.TeamFoundationCollectionUri)$(System.TeamProject)/_build/results?buildId=$(Build.BuildId)'
```
**What to do:**
- Add this step after your PR deployment and test jobs to report the result to the PR.

---

## Step 4: Cleaning Up PR Environments with Webhook Triggers

When a PR is closed or abandoned, you should clean up the temporary namespace and resources. This is done using a webhook trigger and a dedicated cleanup pipeline.

### 4.1. Webhook Setup

- Configure an incoming webhook in Azure DevOps that triggers on PR updates (e.g., completed or abandoned events).
- The webhook triggers the `pr-cleanup.yml` pipeline and passes PR details as parameters.

### 4.2. Cleanup Pipeline: `pr-cleanup.yml`

**File:** `pipelines/pr-cleanup.yml`

**Purpose:**
- Deletes the Kubernetes namespace for the PR when the PR is completed or abandoned.

**Full Example:**
```yaml
trigger: none

variables:
  - template: config/var-pool.yml
  - template: config/var-commonvariables.yml
  - template: config/var-commonvariables-release.yml

resources:
  webhooks:
    - webhook: fastfoodPrUpdated
      connection: FastfoodPREventsConnection
      filters:
      - path: eventType
        value: git.pullrequest.updated
      - path: publisherId
        value: tfs
      - path: resource.repository.name
        value: FastFood
  containers:
    - container: worker
      image: $(azureContainerRegistry)/fastfood-buildenv:$(NetCoreSdkVersion)
      endpoint: 4taksDemoAcr

jobs:
  - job: prCleanup
    displayName: 'PR Cleanup'
    condition: or(eq('${{ parameters.fastfoodPrUpdated.resource.status }}', 'completed'), eq('${{ parameters.fastfoodPrUpdated.resource.status }}', 'abandoned'))
    pool:
      vmImage: 'ubuntu-latest'
    steps:
    - checkout: none
    - task: Kubernetes@1
      displayName: 'Delete PR Kubernetes Namespace'
      inputs:
        connectionType: 'Kubernetes Service Connection'
        kubernetesServiceConnection: $(kubernetesDeploymentServiceConnection)
        command: delete
        arguments: 'namespace pr-${{ parameters.fastfoodPrUpdated.resource.pullRequestId }}'
```
**Explanation:**
- The pipeline is triggered by a webhook when a PR is updated.
- It checks if the PR is completed or abandoned, and if so, deletes the corresponding Kubernetes namespace.

---

## Step 5: Summary

- **Staging** and **Production** stages are used for mainline deployments, with Production depending on Staging.
- **PullRequest** stage is used for PR validation, deploying to a unique namespace and running verification jobs.
- Custom PR status checks can be posted using a step template and the Azure DevOps REST API.
- PR environments are automatically cleaned up using a webhook-triggered pipeline.

This approach ensures that every PR is validated in isolation, results are visible in the PR, and resources are cleaned up automatically.

If you need help with PR environments, webhooks, or status checks, refer to the official Azure DevOps documentation or ask your instructor.
