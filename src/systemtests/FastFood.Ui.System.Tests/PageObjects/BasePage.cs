using Microsoft.Playwright;

namespace FastFood.Ui.System.Tests.PageObjects;

/// <summary>
/// Base class for all page objects
/// 
/// BEST PRACTICE: Page objects should encapsulate all UI automation details.
/// Testers should use the public methods provided by page objects and avoid accessing
/// the Page property directly. The Page property is marked as internal to enforce this.
/// </summary>
public abstract class BasePage
{
    /// <summary>
    /// The underlying Playwright page object.
    /// INTERNAL: Use page object methods instead of accessing this directly from tests.
    /// </summary>
    internal IPage Page { get; }
    
    protected readonly string BaseUrl;

    protected BasePage(IPage page, string baseUrl)
    {
        Page = page;
        BaseUrl = baseUrl;
    }

    /// <summary>
    /// Navigate to the page
    /// </summary>
    public virtual async Task NavigateAsync()
    {
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
    }

    #region Protected Helper Methods for Page Object Implementations

    /// <summary>
    /// Wait for element to be visible
    /// </summary>
    protected async Task WaitForElementAsync(string selector, int? timeout = null)
    {
        await Page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = timeout
        });
    }

    /// <summary>
    /// Get text content of an element
    /// </summary>
    protected async Task<string?> GetTextContentAsync(string selector)
    {
        var element = await Page.QuerySelectorAsync(selector);
        return element != null ? await element.TextContentAsync() : null;
    }

    /// <summary>
    /// Check if element exists
    /// </summary>
    protected async Task<bool> ElementExistsAsync(string selector)
    {
        var element = await Page.QuerySelectorAsync(selector);
        return element != null;
    }

    #endregion
}
