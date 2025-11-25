using Microsoft.Playwright;

namespace FastFood.Ui.System.Tests.PageObjects.SelfServicePos;

/// <summary>
/// Page object for the Self Service POS Welcome page
/// URL: /
/// 
/// BEST PRACTICE: This page object encapsulates all UI automation details.
/// Testers should only use the public methods provided and never access the Page property directly.
/// Navigation methods return the next page object, creating a fluent interface.
/// </summary>
public class WelcomePage : BasePage
{
    public WelcomePage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    public override async Task NavigateAsync()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    /// <summary>
    /// Clicks the "Start Ordering" button and navigates to the products page.
    /// Returns a ProductsPage object representing the new browser state.
    /// </summary>
    /// <returns>ProductsPage object for the next step in the workflow</returns>
    public async Task<ProductsPage> StartOrderingAsync()
    {
        var button = Page.GetByTestId("start-ordering-button");
        await button.ClickAsync();
        
        // SPA: Wait for URL to change (no navigation event, just Vue Router)
        await Page.WaitForURLAsync("**/products", new() { WaitUntil = WaitUntilState.DOMContentLoaded });
        
        // Wait for the products page content to be visible
        await Page.GetByTestId("shopping-cart").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 10000 });
        
        return new ProductsPage(Page, BaseUrl);
    }

    /// <summary>
    /// Verifies whether the welcome screen is currently displayed.
    /// </summary>
    /// <returns>True if on welcome page, false otherwise</returns>
    public async Task<bool> IsOnWelcomePageAsync()
    {
        var count = await Page.GetByTestId("welcome-screen").CountAsync();
        return count > 0;
    }
}
