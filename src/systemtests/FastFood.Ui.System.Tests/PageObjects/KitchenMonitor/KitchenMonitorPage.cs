using Microsoft.Playwright;

namespace FastFood.Ui.System.Tests.PageObjects.KitchenMonitor;

/// <summary>
/// Page Object for Kitchen Monitor
/// 
/// BEST PRACTICE: This page object hides all UI automation complexity from testers.
/// Testers use simple methods like WaitForOrder(), FinishItem(), etc.
/// Uses dual lookup strategy for robustness:
/// - Item IDs (GUIDs) for stable, unique identification
/// - Display names for test readability and human-friendly assertions
/// 
/// Data attributes on order items:
/// - data-testid: Uses item.id (GUID) for unique identification
/// - data-order-ref: Contains order reference for filtering/debugging
/// - data-product-name: Contains display name for reference/debugging
/// </summary>
public class KitchenMonitorPage : BasePage
{
    public KitchenMonitorPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    /// <summary>
    /// Waits for an order to appear in the kitchen monitor.
    /// Use this method to verify that an order has been submitted successfully.
    /// </summary>
    /// <param name="orderNumber">The order number to wait for (e.g., "O12345")</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds (default: 30 seconds)</param>
    /// <returns>True if order appears, false if timeout</returns>
    public async Task<bool> WaitForOrderAsync(string orderNumber, int timeoutMs = 30000)
    {
        try
        {
            var orderTestId = $"order-card-{orderNumber.ToLower()}";
            await Page.GetByTestId(orderTestId).WaitForAsync(new() { Timeout = timeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets the details of an order including all its items.
    /// Useful for verification and deciding which items to finish.
    /// </summary>
    /// <param name="orderNumber">The order number (e.g., "O12345")</param>
    /// <returns>KitchenOrder object with items, or null if order not found</returns>
    public async Task<KitchenOrder?> GetOrderAsync(string orderNumber)
    {
        var orderTestId = $"order-card-{orderNumber.ToLower()}";
        var orderCard = Page.GetByTestId(orderTestId);
        
        if (await orderCard.CountAsync() == 0)
        {
            return null;
        }

        var order = new KitchenOrder
        {
            OrderNumber = orderNumber,
            Items = new List<KitchenOrderItem>()
        };

        // Find all order items - now using item.id with data-order-ref filter
        var itemLocators = orderCard.Locator($"[data-order-ref='{orderNumber.ToLower()}']");
        var count = await itemLocators.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var itemLocator = itemLocators.Nth(i);
            
            // Get the item ID from data-testid
            var testId = await itemLocator.GetAttributeAsync("data-testid");
            var itemId = testId?.StartsWith("order-item-") == true ? testId.Substring("order-item-".Length) : null;
            
            // Get the product name from data attribute
            var productName = await itemLocator.GetAttributeAsync("data-product-name") ?? "";
            productName = productName.Trim();
            
            // Check if item is finished
            var finishedLabel = itemId != null ? itemLocator.Page.GetByTestId($"item-finished-{itemId}") : null;
            var isFinished = finishedLabel != null && await finishedLabel.CountAsync() > 0;

            if (!string.IsNullOrEmpty(productName) && itemId != null)
            {
                order.Items.Add(new KitchenOrderItem
                {
                    ItemId = itemId,
                    ProductName = productName,
                    Quantity = 1, // Kitchen doesn't display quantity in the current UI
                    IsFinished = isFinished
                });
            }
        }

        return order;
    }

    /// <summary>
    /// Marks a specific item as finished in the kitchen.
    /// The item is identified by its product name within the order context.
    /// Testers can use product names they see in the UI.
    /// </summary>
    /// <param name="orderNumber">The order number containing the item</param>
    /// <param name="productName">The product name to finish (e.g., "Cheeseburger")</param>
    public async Task FinishItemAsync(string orderNumber, string productName)
    {
        // Find the item by order reference and product name
        var itemLocator = Page.Locator(
            $"[data-order-ref='{orderNumber.ToLower()}'][data-product-name='{productName}']"
        );
        
        if (await itemLocator.CountAsync() == 0)
        {
            throw new InvalidOperationException($"Item '{productName}' not found in order '{orderNumber}'");
        }
        
        // Get the item ID from the data-testid attribute
        var testId = await itemLocator.First.GetAttributeAsync("data-testid");
        if (testId != null && testId.StartsWith("order-item-"))
        {
            var itemId = testId.Substring("order-item-".Length);
            await FinishItemByIdAsync(itemId);
        }
        else
        {
            throw new InvalidOperationException($"Could not determine item ID for '{productName}'");
        }
    }

    /// <summary>
    /// Finishes all items in an order.
    /// Optionally leaves one item pending to test partial completion scenarios.
    /// </summary>
    /// <param name="orderNumber">The order number</param>
    /// <param name="leaveOnePending">If true, leaves the last item unfinished (default: false)</param>
    public async Task FinishAllItemsAsync(string orderNumber, bool leaveOnePending = false)
    {
        var order = await GetOrderAsync(orderNumber);
        if (order == null) return;

        var itemsToFinish = order.Items.Where(i => !i.IsFinished).ToList();
        var count = leaveOnePending ? itemsToFinish.Count - 1 : itemsToFinish.Count;

        for (int i = 0; i < count; i++)
        {
            await FinishItemAsync(orderNumber, itemsToFinish[i].ProductName);
        }
    }

    /// <summary>
    /// Checks if an order is currently visible in the kitchen monitor.
    /// </summary>
    /// <param name="orderNumber">The order number to check</param>
    /// <returns>True if order exists, false otherwise</returns>
    public async Task<bool> OrderExistsAsync(string orderNumber)
    {
        var orderTestId = $"order-card-{orderNumber.ToLower()}";
        var count = await Page.GetByTestId(orderTestId).CountAsync();
        return count > 0;
    }

    /// <summary>
    /// Waits for an order to disappear from the kitchen monitor.
    /// This happens when all items are finished and the order is complete.
    /// </summary>
    /// <param name="orderNumber">The order number to wait for</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds (default: 30 seconds)</param>
    /// <returns>True if order disappears, false if timeout</returns>
    public async Task<bool> WaitForOrderDisappearAsync(string orderNumber, int timeoutMs = 30000)
    {
        try
        {
            var orderTestId = $"order-card-{orderNumber.ToLower()}";
            var orderLocator = Page.GetByTestId(orderTestId);
            await orderLocator.WaitForAsync(new() { State = WaitForSelectorState.Detached, Timeout = timeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }

    #region Internal Helper Methods

    /// <summary>
    /// Finish a specific item by item ID (GUID).
    /// Internal method - testers should use FinishItemAsync() with product name.
    /// </summary>
    internal async Task FinishItemByIdAsync(string itemId)
    {
        var finishButtonTestId = $"finish-button-{itemId}";
        
        var finishButton = Page.GetByTestId(finishButtonTestId);
        await finishButton.ClickAsync();
        
        // Wait for the UI to update
        await Task.Delay(500, Xunit.TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Find item ID by order number and product display name.
    /// Internal helper for ID-based operations.
    /// </summary>
    internal async Task<string?> FindItemIdByProductNameAsync(string orderNumber, string productName)
    {
        var itemLocator = Page.Locator(
            $"[data-order-ref='{orderNumber.ToLower()}'][data-product-name='{productName}']"
        );
        
        if (await itemLocator.CountAsync() == 0)
        {
            return null;
        }
        
        var testId = await itemLocator.First.GetAttributeAsync("data-testid");
        if (testId != null && testId.StartsWith("order-item-"))
        {
            return testId.Substring("order-item-".Length);
        }
        
        return null;
    }

    /// <summary>
    /// Get product name by item ID.
    /// Internal helper for reverse lookup.
    /// </summary>
    internal async Task<string?> GetProductNameByItemIdAsync(string itemId)
    {
        var itemLocator = Page.GetByTestId($"order-item-{itemId}");
        if (await itemLocator.CountAsync() == 0)
        {
            return null;
        }
        
        return await itemLocator.GetAttributeAsync("data-product-name");
    }

    #endregion
}

public class KitchenOrder
{
    public string OrderNumber { get; set; } = string.Empty;
    public List<KitchenOrderItem> Items { get; set; } = new();
}

public class KitchenOrderItem
{
    public string? ItemId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public bool IsFinished { get; set; }
}
