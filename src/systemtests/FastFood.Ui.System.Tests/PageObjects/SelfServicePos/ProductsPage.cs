using Microsoft.Playwright;

namespace FastFood.Ui.System.Tests.PageObjects.SelfServicePos;

/// <summary>
/// Page object for the Self Service POS Products page
/// URL: /products
/// 
/// BEST PRACTICE: This page object encapsulates all UI automation details.
/// Testers use simple methods like AddProduct() and Checkout() without knowing the underlying selectors.
/// Uses dual lookup strategy for robustness:
/// - Product IDs (GUIDs) for stable, unique, translation-independent identification
/// - Display names for test readability and human-friendly assertions
/// 
/// Data attributes on product cards:
/// - data-testid: Uses product.id (GUID) for unique identification
/// - data-product-name: Contains display name for reference/debugging
/// </summary>
public class ProductsPage : BasePage
{
    public ProductsPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    public override async Task NavigateAsync()
    {
        await Page.GotoAsync($"{BaseUrl}products");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Adds a product to the shopping cart by its display name.
    /// Testers can use product names they see in the UI without knowing internal IDs.
    /// </summary>
    /// <param name="productName">The display name of the product (e.g., "Cheeseburger")</param>
    /// <param name="quantity">Number of items to add (default: 1)</param>
    public async Task AddProductAsync(string productName, int quantity = 1)
    {
        // Wait for the shopping cart to be visible/ready (indicates page is loaded)
        await Page.GetByTestId("shopping-cart").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        
        // Get current cart item count before adding
        var initialCount = await GetCartItemCountAsync();
        
        // Find product by display name first
        var productId = await FindProductIdByDisplayNameAsync(productName);
        if (productId == null)
        {
            throw new InvalidOperationException($"Product '{productName}' not found on the products page");
        }
        
        // Adjust quantity if needed
        if (quantity > 1)
        {
            var plusButton = Page.GetByTestId($"increase-quantity-{productId}");
            for (int i = 1; i < quantity; i++)
            {
                await plusButton.ClickAsync();
                await Task.Delay(100, Xunit.TestContext.Current.CancellationToken); // Small delay for UI update
            }
        }

        // Click Add button using product ID
        var addButton = Page.GetByTestId($"add-to-cart-{productId}");
        await addButton.ClickAsync();
        
        // Wait for the cart to update - the cart count should increase
        for (int i = 0; i < 30; i++) // Try for up to 15 seconds
        {
            await Task.Delay(500, Xunit.TestContext.Current.CancellationToken);
            var newCount = await GetCartItemCountAsync();
            if (newCount > initialCount)
            {
                // Cart has been updated!
                return;
            }
        }
        
        // If we get here, cart didn't update - but don't fail, let the test assertion handle it
    }

    /// <summary>
    /// Removes a product from the cart by its display name.
    /// </summary>
    /// <param name="productName">The display name of the product to remove</param>
    public async Task RemoveProductAsync(string productName)
    {
        var cartItemLocator = Page.Locator($"text=/{productName} - \\d+ x/");
        var removeButton = cartItemLocator.Locator("..").GetByRole(AriaRole.Button, new() { Name = "Remove" });
        await removeButton.ClickAsync();
        await Task.Delay(300, Xunit.TestContext.Current.CancellationToken);
    }

    /// <summary>
    /// Gets the current items in the shopping cart.
    /// Useful for verification before checkout.
    /// </summary>
    /// <returns>List of cart items with product details</returns>
    public async Task<List<CartItem>> GetCartItemsAsync()
    {
        // Wait for cart to potentially update - try multiple times
        for (int attempt = 0; attempt < 10; attempt++)
        {
            var items = await TryGetCartItemsAsync();
            if (items.Count > 0)
            {
                return items;
            }
            
            // Wait a bit and try again
            await Page.WaitForTimeoutAsync(500);
        }
        
        // Last attempt
        return await TryGetCartItemsAsync();
    }

    /// <summary>
    /// Gets the current total price of items in the cart.
    /// </summary>
    /// <returns>Total price as decimal</returns>
    public async Task<decimal> GetTotalAsync()
    {
        var totalText = await Page.Locator("strong:has-text('Total:')").Locator("..").TextContentAsync() ?? "";
        var match = global::System.Text.RegularExpressions.Regex.Match(totalText, @"\$([0-9.]+)");
        if (match.Success)
        {
            return decimal.Parse(match.Groups[1].Value);
        }
        return 0;
    }

    /// <summary>
    /// Checks whether the checkout button is enabled.
    /// Useful for validating business rules (e.g., empty cart cannot be ordered).
    /// </summary>
    /// <returns>True if order button is enabled, false otherwise</returns>
    public async Task<bool> IsCheckoutEnabledAsync()
    {
        var orderButton = Page.GetByTestId("order-button");
        var count = await orderButton.CountAsync();
        return count > 0 && await orderButton.IsEnabledAsync();
    }

    /// <summary>
    /// Proceeds to checkout by clicking the Order button.
    /// Returns an OrderConfirmationPage object representing the next step in the workflow.
    /// </summary>
    /// <returns>OrderConfirmationPage object for order review and payment</returns>
    public async Task<OrderConfirmationPage> CheckoutAsync()
    {
        await Page.GetByTestId("order-button").ClickAsync();
        
        // SPA: Wait for URL to change (no navigation event, just Vue Router)
        await Page.WaitForURLAsync("**/order-confirmation", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        
        // Wait for order confirmation page to appear
        await Page.GetByTestId("order-confirmation-page").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        
        return new OrderConfirmationPage(Page, BaseUrl);
    }

    #region Internal Helper Methods

    /// <summary>
    /// Find product ID by display name.
    /// Uses data-product-name attribute for lookup.
    /// </summary>
    internal async Task<string?> FindProductIdByDisplayNameAsync(string productName)
    {
        var productCards = Page.Locator($"[data-product-name='{productName}']");
        var count = await productCards.CountAsync();
        
        if (count == 0)
        {
            return null;
        }
        
        // Extract the product ID from the data-testid attribute
        var testId = await productCards.First.GetAttributeAsync("data-testid");
        if (testId != null && testId.StartsWith("product-card-"))
        {
            return testId.Substring("product-card-".Length);
        }
        
        return null;
    }

    /// <summary>
    /// Get product display name by product ID.
    /// </summary>
    internal async Task<string?> GetProductNameByIdAsync(string productId)
    {
        var productCard = Page.GetByTestId($"product-card-{productId}");
        if (await productCard.CountAsync() == 0)
        {
            return null;
        }
        
        return await productCard.GetAttributeAsync("data-product-name");
    }

    /// <summary>
    /// Get all available products with their IDs and names.
    /// </summary>
    internal async Task<List<ProductInfo>> GetAvailableProductsAsync()
    {
        var products = new List<ProductInfo>();
        var productCards = Page.Locator("[data-testid^='product-card-']");
        var count = await productCards.CountAsync();
        
        for (int i = 0; i < count; i++)
        {
            var card = productCards.Nth(i);
            var testId = await card.GetAttributeAsync("data-testid");
            var displayName = await card.GetAttributeAsync("data-product-name");
            
            if (testId != null && testId.StartsWith("product-card-") && displayName != null)
            {
                var productId = testId.Substring("product-card-".Length);
                products.Add(new ProductInfo
                {
                    ProductId = productId,
                    DisplayName = displayName
                });
            }
        }
        
        return products;
    }

    private async Task<int> GetCartItemCountAsync()
    {
        // Check if cart is empty
        var emptyCart = await Page.GetByTestId("empty-cart-message").CountAsync();
        if (emptyCart > 0)
        {
            return 0;
        }

        // Count cart items - now using item.id based data-testid
        var cartItems = Page.Locator("[data-testid^='cart-item-']:not([data-testid*='-text'])");
        return await cartItems.CountAsync();
    }

    private async Task<List<CartItem>> TryGetCartItemsAsync()
    {
        var items = new List<CartItem>();
        
        // Check if cart is empty
        var emptyCart = await Page.GetByTestId("empty-cart-message").CountAsync();
        if (emptyCart > 0)
        {
            return items;
        }

        // Get all cart item text elements - now using item.id
        var cartItemTexts = Page.Locator("[data-testid^='cart-item-text-']");
        var count = await cartItemTexts.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var textElement = cartItemTexts.Nth(i);
            
            // Get the parent cart item to access data attributes
            var parentLocator = textElement.Locator("..");
            var productId = await parentLocator.GetAttributeAsync("data-product-id");
            var productName = await parentLocator.GetAttributeAsync("data-product-name");
            
            var itemText = await textElement.TextContentAsync() ?? "";
            
            var item = ParseCartItem(itemText, productId, productName);
            if (item != null)
            {
                items.Add(item);
            }
        }

        return items;
    }

    private CartItem? ParseCartItem(string itemText, string? productId, string? productName)
    {
        // Format: "ProductName - 1 x $6.99 = $6.99"
        var regex = new global::System.Text.RegularExpressions.Regex(@"^(.+?)\s*-\s*(\d+)\s*x\s*\$([0-9.]+)\s*=\s*\$([0-9.]+)$");
        var match = regex.Match(itemText.Trim());

        if (match.Success)
        {
            return new CartItem
            {
                ProductId = productId,
                ProductName = match.Groups[1].Value.Trim(),
                Quantity = int.Parse(match.Groups[2].Value),
                UnitPrice = decimal.Parse(match.Groups[3].Value),
                TotalPrice = decimal.Parse(match.Groups[4].Value)
            };
        }

        return null;
    }

    #endregion
}

/// <summary>
/// Product information for lookup and reference
/// </summary>
public class ProductInfo
{
    public string ProductId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

public class CartItem
{
    public string? ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}
