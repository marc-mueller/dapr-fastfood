using Microsoft.Playwright;

namespace FastFood.Ui.System.Tests.PageObjects.SelfServicePos;

/// <summary>
/// Page object for the Payment Confirmation section
/// Appears on the order-confirmation page after clicking Pay
/// 
/// BEST PRACTICE: Encapsulates the payment success flow.
/// Shows success message and provides navigation back to welcome page via ReturnToWelcome() method.
/// </summary>
public class PaymentConfirmationPage : BasePage
{
    public PaymentConfirmationPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    public override async Task NavigateAsync()
    {
        // Cannot navigate directly to payment confirmation
        throw new NotSupportedException("Cannot navigate directly to payment confirmation. Must go through order flow.");
    }

    /// <summary>
    /// Verifies that the payment was confirmed successfully.
    /// </summary>
    /// <returns>True if payment confirmation message is displayed, false otherwise</returns>
    public async Task<bool> IsPaymentConfirmedAsync()
    {
        var confirmationText = await Page.GetByTestId("payment-confirmed-message").CountAsync();
        return confirmationText > 0;
    }

    /// <summary>
    /// Returns to the welcome page by clicking the OK button.
    /// Completes the order workflow and prepares for the next order.
    /// </summary>
    /// <returns>WelcomePage object ready to start a new order</returns>
    public async Task<WelcomePage> ReturnToWelcomeAsync()
    {
        var okButton = Page.GetByTestId("ok-button");
        
        // Wait for OK button to be visible before clicking
        await okButton.WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        await okButton.ClickAsync();
        
        // SPA: Wait for URL to change to welcome page
        await Page.WaitForURLAsync(url => url == BaseUrl || url == $"{BaseUrl}/", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = 5000 });
        
        // Wait for welcome page to appear
        await Page.GetByTestId("start-ordering-button").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        return new WelcomePage(Page, BaseUrl);
    }

    /// <summary>
    /// Waits for the OK button to become available.
    /// Internal helper for validation scenarios.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds</param>
    /// <returns>True if OK button appears, false if timeout</returns>
    internal async Task<bool> WaitForOkButtonAsync(int timeoutMs = 5000)
    {
        try
        {
            await Page.GetByTestId("ok-button").WaitForAsync(new() { Timeout = timeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for automatic redirect to welcome page (alternative to clicking OK).
    /// Some implementations may automatically redirect after payment.
    /// </summary>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds</param>
    /// <returns>WelcomePage object after redirect completes</returns>
    internal async Task<WelcomePage> WaitForAutoRedirectAsync(int timeoutMs = 10000)
    {
        // SPA: Wait for automatic redirect to welcome page
        await Page.WaitForURLAsync(url => url == BaseUrl || url == $"{BaseUrl}/", new() { WaitUntil = WaitUntilState.DOMContentLoaded, Timeout = timeoutMs });
        
        // Wait for welcome page to appear
        await Page.GetByTestId("start-ordering-button").WaitForAsync(new() { State = WaitForSelectorState.Visible, Timeout = 5000 });
        
        return new WelcomePage(Page, BaseUrl);
    }
}
