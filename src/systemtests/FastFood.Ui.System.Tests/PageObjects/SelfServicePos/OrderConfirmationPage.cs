using Microsoft.Playwright;

namespace FastFood.Ui.System.Tests.PageObjects.SelfServicePos;

/// <summary>
/// Page object for the Self Service POS Order Confirmation page
/// URL: /order-confirmation
/// 
/// BEST PRACTICE: This page object hides UI automation complexity from testers.
/// Shows order number, items, and payment options.
/// The PayForOrder() method handles payment and returns the next page object in the workflow.
/// </summary>
public class OrderConfirmationPage : BasePage
{
    public OrderConfirmationPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    public override async Task NavigateAsync()
    {
        await Page.GotoAsync($"{BaseUrl}order-confirmation");
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Gets the order number from the confirmation page.
    /// Useful for tracking the order through subsequent steps.
    /// </summary>
    /// <returns>Order number (e.g., "O12345") or null if not found</returns>
    public async Task<string?> GetOrderNumberAsync()
    {
        var heading = Page.GetByTestId("order-confirmation-title");
        var text = await heading.TextContentAsync() ?? "";
        
        // Extract order number from "Order Confirmation (O123)"
        var regex = new global::System.Text.RegularExpressions.Regex(@"\(O(\d+)\)");
        var match = regex.Match(text);
        
        if (match.Success)
        {
            return $"O{match.Groups[1].Value}";
        }
        
        return null;
    }

    /// <summary>
    /// Gets the list of items in the order for verification.
    /// </summary>
    /// <returns>List of order items with quantity, name, and price</returns>
    public async Task<List<OrderItem>> GetOrderItemsAsync()
    {
        var items = new List<OrderItem>();
        
        // Find all order items using test IDs
        var itemLocators = Page.Locator("[data-testid^='order-item-']");
        var count = await itemLocators.CountAsync();

        for (int i = 0; i < count; i++)
        {
            var item = itemLocators.Nth(i);
            
            var quantityText = await item.GetByTestId("item-quantity").TextContentAsync() ?? "0";
            var productName = await item.GetByTestId("item-name").TextContentAsync() ?? "";
            var priceText = await item.GetByTestId("item-total").TextContentAsync() ?? "$0";

            items.Add(new OrderItem
            {
                Quantity = int.Parse(quantityText.Trim()),
                ProductName = productName.Trim(),
                Price = decimal.Parse(priceText.Replace("$", "").Trim())
            });
        }

        return items;
    }

    /// <summary>
    /// Gets the total amount for the order.
    /// </summary>
    /// <returns>Total price as decimal</returns>
    public async Task<decimal> GetTotalAsync()
    {
        var totalText = await Page.GetByTestId("order-total").TextContentAsync() ?? "";
        var match = global::System.Text.RegularExpressions.Regex.Match(totalText, @"\$([0-9.]+)");
        if (match.Success)
        {
            return decimal.Parse(match.Groups[1].Value);
        }
        return 0;
    }

    /// <summary>
    /// Proceeds with payment by clicking the Pay button.
    /// Waits for payment processing to complete and returns a PaymentConfirmationPage object.
    /// </summary>
    /// <returns>PaymentConfirmationPage object showing payment success</returns>
    public async Task<PaymentConfirmationPage> PayForOrderAsync()
    {
        await Page.GetByTestId("pay-button").ClickAsync();
        
        // SPA: No navigation, wait for payment to process and confirmation to appear
        await Page.GetByTestId("payment-confirmation").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 15000 });
        
        return new PaymentConfirmationPage(Page, BaseUrl);
    }
}

public class OrderItem
{
    public int Quantity { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
