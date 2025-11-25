using FastFood.Ui.System.Tests.Configuration;
using FastFood.Ui.System.Tests.PageObjects.CustomerOrderStatus;
using FastFood.Ui.System.Tests.PageObjects.KitchenMonitor;
using FastFood.Ui.System.Tests.PageObjects.SelfServicePos;
using Microsoft.Playwright;

namespace FastFood.Ui.System.Tests.Helpers;

/// <summary>
/// Helper class to initialize browser tabs and return the corresponding page objects.
/// This provides a clean entry point for testers to start their test scenarios without
/// dealing with browser automation details.
/// 
/// BEST PRACTICE: Testers call these helper methods to get initial page objects,
/// then use only page object methods to navigate and interact with the application.
/// </summary>
public static class BrowserHelper
{
    /// <summary>
    /// Opens the Self Service POS in a new browser tab and returns the WelcomePage page object.
    /// The browser will be positioned at the POS welcome screen, ready to start ordering.
    /// </summary>
    /// <param name="context">The browser context to create the page in</param>
    /// <param name="config">Test configuration containing URLs</param>
    /// <returns>WelcomePage object ready for interaction</returns>
    public static async Task<WelcomePage> OpenSelfServicePosAsync(IBrowserContext context, TestConfiguration config)
    {
        var page = await context.NewPageAsync();
        await RegisterVideoFileRenaming(page, "self-service-pos");
        var welcomePage = new WelcomePage(page, config.SelfServicePosUrl);
        await welcomePage.NavigateAsync();
        return welcomePage;
    }
    
    /// <summary>
    /// Opens the Kitchen Monitor in a new browser tab and returns the KitchenMonitorPage page object.
    /// The browser will be positioned at the kitchen monitor, showing active orders.
    /// </summary>
    /// <param name="context">The browser context to create the page in</param>
    /// <param name="config">Test configuration containing URLs</param>
    /// <returns>KitchenMonitorPage object ready for interaction</returns>
    public static async Task<KitchenMonitorPage> OpenKitchenMonitorAsync(IBrowserContext context, TestConfiguration config)
    {
        var page = await context.NewPageAsync();
        await RegisterVideoFileRenaming(page, "kitchen-monitor");
        var kitchenPage = new KitchenMonitorPage(page, config.KitchenMonitorUrl);
        await kitchenPage.NavigateAsync();
        return kitchenPage;
    }

    /// <summary>
    /// Opens the Customer Order Status in a new browser tab and returns the CustomerOrderStatusPage page object.
    /// The browser will be positioned at the customer order status view.
    /// </summary>
    /// <param name="context">The browser context to create the page in</param>
    /// <param name="config">Test configuration containing URLs</param>
    /// <returns>CustomerOrderStatusPage object ready for interaction</returns>
    public static async Task<CustomerOrderStatusPage> OpenCustomerOrderStatusAsync(IBrowserContext context, TestConfiguration config)
    {
        var page = await context.NewPageAsync();
        await RegisterVideoFileRenaming(page, "customer-order-status");
        var orderStatusPage = new CustomerOrderStatusPage(page, config.CustomerOrderStatusUrl);
        await orderStatusPage.NavigateAsync();
        return orderStatusPage;
    }

    private static async Task RegisterVideoFileRenaming(IPage page, string appName)
    {
        var videoPath = await page.Video.PathAsync();
        var fileName = Path.GetFileName(videoPath);
        var fileExtension = Path.GetExtension(videoPath);
        var testContext = TestContext.Current;
        var newFilename = $"{testContext.TestClass}_{testContext.TestMethod}_{appName}{fileExtension}";
        testContext.KeyValueStorage.AddOrUpdate(fileName, k => newFilename
            , (k, old) => newFilename);

    }
}
