using FastFood.Ui.System.Tests.PageObjects.SelfServicePos;
using FastFood.Ui.System.Tests.Base;
using FastFood.Ui.System.Tests.PageObjects.CustomerOrderStatus;
using FastFood.Ui.System.Tests.PageObjects.KitchenMonitor;
using FastFood.Ui.System.Tests.Helpers;
using Xunit;

namespace FastFood.Ui.System.Tests;

/// <summary>
/// Full end-to-end order workflow test
/// Tests the complete order lifecycle across all three frontends
/// 
/// BEST PRACTICES DEMONSTRATED:
/// 1. Tests use BrowserHelper to initialize browser tabs and get page objects as local variables
/// 2. All interactions happen through page object methods (no direct Page access)
/// 3. Navigation methods return new page objects, creating a fluent interface
/// 4. Data-testid attributes use unique IDs (GUIDs) for stable, translation-independent selectors
/// 5. Page objects provide dual lookup: by display name (human-readable) and by ID (stable)
/// 6. Tests use display names for readability, page objects handle ID resolution internally
/// 7. All elements include data-* attributes for debugging (e.g., data-product-name, data-order-ref)
/// 
/// IMPORTANT: All three tabs must remain open throughout the test for SignalR to work properly.
/// Videos are automatically recorded by PageTest and uploaded to Azure DevOps.
/// </summary>
public class OrderWorkflowTests : PlaywrightTestBase
{
    [Fact]
    public async Task CompleteOrderWorkflow_ShouldProcessOrderThroughAllStages()
    {
        // Arrange - Initialize browser tabs with page objects as local variables
        var posPage = await BrowserHelper.OpenSelfServicePosAsync(Context, Configuration);
        var kitchenPage = await BrowserHelper.OpenKitchenMonitorAsync(Context, Configuration);
        var orderStatusPage = await BrowserHelper.OpenCustomerOrderStatusAsync(Context, Configuration);

        // Act - Step 1: Start ordering on POS
        var productsPage = await posPage.StartOrderingAsync();
        
        // Step 2: Add products to cart
        await productsPage.AddProductAsync("Cheeseburger", 1);
        await productsPage.AddProductAsync("Classic Fries", 1);
        await productsPage.AddProductAsync("Cola", 2);

        // Step 3: Verify cart contents
        var cartItems = await productsPage.GetCartItemsAsync();
        Assert.Equal(3, cartItems.Count);
        Assert.Contains(cartItems, i => i.ProductName == "Cheeseburger" && i.Quantity == 1);
        Assert.Contains(cartItems, i => i.ProductName == "Classic Fries" && i.Quantity == 1);
        Assert.Contains(cartItems, i => i.ProductName == "Cola" && i.Quantity == 2);

        // Step 4: Proceed to checkout
        var confirmationPage = await productsPage.CheckoutAsync();
        
        // Step 5: Verify order confirmation
        var orderNumber = await confirmationPage.GetOrderNumberAsync();
        Assert.NotNull(orderNumber);
        Assert.StartsWith("O", orderNumber);

        var confirmationItems = await confirmationPage.GetOrderItemsAsync();
        Assert.Equal(3, confirmationItems.Count);

        // Step 6: Pay for order
        var paymentPage = await confirmationPage.PayForOrderAsync();
        
        // Step 7: Verify payment confirmed and return to welcome
        var isConfirmed = await paymentPage.IsPaymentConfirmedAsync();
        Assert.True(isConfirmed, "Payment should be confirmed");
        
        posPage = await paymentPage.ReturnToWelcomeAsync();

        // Step 8: Verify order appears in kitchen monitor
        var orderAppeared = await kitchenPage.WaitForOrderAsync(orderNumber, 30000);
        Assert.True(orderAppeared, $"Order {orderNumber} did not appear in kitchen monitor");

        // Step 9: Get kitchen order details and verify all items
        var kitchenOrder = await kitchenPage.GetOrderAsync(orderNumber);
        Assert.NotNull(kitchenOrder);
        Assert.Equal(3, kitchenOrder.Items.Count);
        Assert.Contains(kitchenOrder.Items, i => i.ProductName.Contains("Cheeseburger"));
        Assert.Contains(kitchenOrder.Items, i => i.ProductName.Contains("Classic Fries"));
        Assert.Contains(kitchenOrder.Items, i => i.ProductName.Contains("Cola"));

        // Step 10: Verify order appears in customer order status as "In Preparation"
        var orderInPreparation = await orderStatusPage.WaitForOrderInPreparationAsync(orderNumber, 30000);
        Assert.True(orderInPreparation, $"Order {orderNumber} not shown in preparation");

        // Step 11: Finish all items except one in the kitchen
        var itemsToFinish = kitchenOrder.Items.Where(i => !i.IsFinished).ToList();
        for (int i = 0; i < itemsToFinish.Count - 1; i++)
        {
            await kitchenPage.FinishItemAsync(orderNumber, itemsToFinish[i].ProductName);
        }

        // Step 12: Verify order is still in progress in customer view
        await Task.Delay(2000, Xunit.TestContext.Current.CancellationToken); // Wait for SignalR updates to propagate
        var stillInPreparation = await orderStatusPage.IsOrderInPreparationAsync(orderNumber);
        Assert.True(stillInPreparation, "Order should still be in preparation with one item pending");

        // Step 13: Finish the last item in the kitchen
        var lastItem = itemsToFinish.Last();
        await kitchenPage.FinishItemAsync(orderNumber, lastItem.ProductName);

        // Step 14: Verify order disappears from kitchen monitor
        var orderDisappeared = await kitchenPage.WaitForOrderDisappearAsync(orderNumber, 30000);
        Assert.True(orderDisappeared, $"Order {orderNumber} did not disappear from kitchen monitor");

        // Step 15: Verify order moved to finished in customer view
        var orderFinished = await orderStatusPage.WaitForOrderToFinishAsync(orderNumber, 30000);
        Assert.True(orderFinished, $"Order {orderNumber} not shown in finished orders");

        var finishedOrder = await orderStatusPage.GetFinishedOrderAsync(orderNumber);
        Assert.NotNull(finishedOrder);
        Assert.Equal(orderNumber, finishedOrder.OrderNumber);
    }

    [Fact]
    public async Task MultipleOrders_ShouldBeIdentifiableByOrderNumber()
    {
        // Arrange - Initialize browser tabs as local variables
        var posPage = await BrowserHelper.OpenSelfServicePosAsync(Context, Configuration);
        var kitchenPage = await BrowserHelper.OpenKitchenMonitorAsync(Context, Configuration);

        // Act - Create first order
        var productsPage = await posPage.StartOrderingAsync();
        await productsPage.AddProductAsync("Veggie Burger", 1);
        var confirmationPage = await productsPage.CheckoutAsync();
        var firstOrderNumber = await confirmationPage.GetOrderNumberAsync();
        Assert.NotNull(firstOrderNumber);
        var paymentPage = await confirmationPage.PayForOrderAsync();
        posPage = await paymentPage.ReturnToWelcomeAsync();

        // Create second order
        productsPage = await posPage.StartOrderingAsync();
        await productsPage.AddProductAsync("Chicken Burger", 1);
        confirmationPage = await productsPage.CheckoutAsync();
        var secondOrderNumber = await confirmationPage.GetOrderNumberAsync();
        Assert.NotNull(secondOrderNumber);
        paymentPage = await confirmationPage.PayForOrderAsync();
        posPage = await paymentPage.ReturnToWelcomeAsync();

        // Assert - Verify both orders appear in kitchen
        Assert.NotEqual(firstOrderNumber, secondOrderNumber);
        
        var firstOrderAppeared = await kitchenPage.WaitForOrderAsync(firstOrderNumber, 20000);
        Assert.True(firstOrderAppeared, $"First order {firstOrderNumber} did not appear");
        
        var secondOrderAppeared = await kitchenPage.WaitForOrderAsync(secondOrderNumber, 20000);
        Assert.True(secondOrderAppeared, $"Second order {secondOrderNumber} did not appear");

        // Verify we can retrieve each order independently
        var firstOrder = await kitchenPage.GetOrderAsync(firstOrderNumber);
        var secondOrder = await kitchenPage.GetOrderAsync(secondOrderNumber);
        
        Assert.NotNull(firstOrder);
        Assert.NotNull(secondOrder);
        Assert.Contains(firstOrder.Items, i => i.ProductName.Contains("Veggie Burger"));
        Assert.Contains(secondOrder.Items, i => i.ProductName.Contains("Chicken Burger"));
    }

    [Fact]
    public async Task EmptyCart_ShouldNotAllowOrder()
    {
        // Arrange - Initialize POS tab (we only need one tab for this test)
        var posPage = await BrowserHelper.OpenSelfServicePosAsync(Context, Configuration);

        // Act - Navigate to products page
        var productsPage = await posPage.StartOrderingAsync();

        // Assert - Checkout button should not be available with empty cart
        var cartItems = await productsPage.GetCartItemsAsync();
        Assert.Empty(cartItems);
        
        var isCheckoutEnabled = await productsPage.IsCheckoutEnabledAsync();
        Assert.False(isCheckoutEnabled, "Checkout button should be disabled when cart is empty");
    }
}
