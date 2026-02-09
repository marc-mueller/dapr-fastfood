---
name: playwright-ui-system-tests-csharp
description: 'Implement or update UI system tests in C# with Playwright for FastFood.Ui.System.Tests. Use when asked to add new UI tests, extend page objects, validate multi-tab workflows across POS, Kitchen Monitor, and Customer Order Status, or add missing data-testid attributes in Vue components to improve test stability.'
---

# Playwright UI System Tests (C#)

A skill for implementing and updating UI system tests in the FastFoodDelivery solution using Playwright and xUnit. The project enforces a strict separation between tests and automation details: tests only call page objects; page objects encapsulate all Playwright selectors and automation logic.

## When to Use This Skill

- Add new UI system tests or update existing Playwright tests
- Implement or extend page objects in C#
- Validate multi-tab flows that rely on SignalR push notifications
- Identify or add missing data-testid attributes in Vue components to improve test stability

## Prerequisites

- Project: src/systemtests/FastFood.Ui.System.Tests
- Playwright uses xUnit v3 with Microsoft.Playwright.Xunit.v3
- Three apps: Self Service POS, Kitchen Monitor, Customer Order Status
- Multi-tab tests are required to keep SignalR connections alive

## Ground Rules

- Tests must only use page object methods; never access IPage directly in tests
- Page objects may use Playwright locators, data-testid, and data-* attributes
- Always prefer stable selectors based on data-testid and data-* attributes
- Keep page objects small and focused; one page object per view/area
- Navigation methods return the next page object to keep a fluent workflow

## Project Conventions (Observed)

- Base test class: Base/PlaywrightTestBase.cs
- Multi-tab helper: Helpers/BrowserHelper.cs
- Page objects: PageObjects/**
- Test configuration: Configuration/TestConfiguration.cs
- data-testid patterns:
  - POS: start-ordering-button, shopping-cart, order-button, product-card-<id>
  - Kitchen: order-card-<orderRef>, order-item-<id>, finish-button-<id>
  - Order status: preparation-order-<orderRef>, finished-order-<orderRef>

## Step-by-Step Workflow

1. Analyze existing tests and page objects
   - Start with OrderWorkflowTests.cs and related page objects.
   - Note how multi-tab setup is done with BrowserHelper.

2. Identify the UI elements and expected behaviors
   - Inspect Vue components for data-testid and data-* attributes.
   - If missing, add data-testid attributes to the frontend.

3. Implement or update page objects
   - Add methods for actions and queries using data-testid.
   - Keep all Playwright selectors in page objects.
   - Use wait helpers and explicit waits where needed.

4. Write or update tests
   - Use BrowserHelper to open the required tabs.
   - Call page object methods only.
   - Use clear, readable assertions in tests.

5. Validate multi-tab behavior
   - Keep all tabs open for SignalR updates.
   - Use wait helpers for order appearance/disappearance.

## Multi-Tab Patterns

- Use BrowserHelper.OpenSelfServicePosAsync, OpenKitchenMonitorAsync, OpenCustomerOrderStatusAsync
- Keep pages alive for the full test to ensure SignalR updates propagate
- Prefer WaitForOrderAsync and WaitForOrderToFinishAsync instead of fixed delays

## Guidance for Adding data-testid

- Use unique, stable data-testid values in Vue templates
- Prefer data-testid and data-* attributes for selectors
- Avoid text selectors unless no stable attribute is available

Example snippet in Vue:

```vue
<div data-testid="product-card-{{ product.id }}" :data-product-name="product.name">
  <button :data-testid="`add-to-cart-${product.id}`">Add</button>
</div>
```

## Testing Checklist

- Tests only use page objects
- Page objects rely on data-testid and data-* attributes
- Multi-tab setup uses BrowserHelper
- URLs come from TestConfiguration
- Assertions verify expected workflow state

## Troubleshooting

- If elements are flaky, prefer explicit waits on data-testid
- If selectors break, check for missing data-testid attributes in Vue
- If SignalR updates are missing, confirm all tabs remain open

## References

- Base/PlaywrightTestBase.cs
- Helpers/BrowserHelper.cs
- PageObjects/**
- OrderWorkflowTests.cs
