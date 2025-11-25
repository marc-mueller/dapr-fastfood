using Microsoft.Playwright;

namespace FastFood.Ui.System.Tests.PageObjects.CustomerOrderStatus;

/// <summary>
/// Page Object for Customer Order Status
/// 
/// BEST PRACTICE: This page object hides UI automation complexity from testers.
/// Testers use simple methods like WaitForOrderInPreparation(), WaitForOrderToFinish(), etc.
/// The page shows orders in two sections: preparation (in progress) and finished.
/// </summary>
public class CustomerOrderStatusPage : BasePage
{
    public CustomerOrderStatusPage(IPage page, string baseUrl) : base(page, baseUrl)
    {
    }

    /// <summary>
    /// Waits for an order to appear in the "In Preparation" section.
    /// Use this to verify that an order has been received and is being prepared.
    /// </summary>
    /// <param name="orderNumber">The order number to wait for (e.g., "O12345")</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds (default: 30 seconds)</param>
    /// <returns>True if order appears in preparation, false if timeout</returns>
    public async Task<bool> WaitForOrderInPreparationAsync(string orderNumber, int timeoutMs = 30000)
    {
        try
        {
            var orderTestId = $"preparation-order-{orderNumber.ToLower()}";
            await Page.GetByTestId(orderTestId).WaitForAsync(new() { Timeout = timeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if an order is currently in the "In Preparation" section.
    /// </summary>
    /// <param name="orderNumber">The order number to check</param>
    /// <returns>True if order is in preparation, false otherwise</returns>
    public async Task<bool> IsOrderInPreparationAsync(string orderNumber)
    {
        var orderTestId = $"preparation-order-{orderNumber.ToLower()}";
        var count = await Page.GetByTestId(orderTestId).CountAsync();
        return count > 0;
    }

    /// <summary>
    /// Checks if an order is in the "Finished Orders" section.
    /// </summary>
    /// <param name="orderNumber">The order number to check</param>
    /// <returns>True if order is finished, false otherwise</returns>
    public async Task<bool> IsOrderFinishedAsync(string orderNumber)
    {
        var orderTestId = $"finished-order-{orderNumber.ToLower()}";
        var count = await Page.GetByTestId(orderTestId).CountAsync();
        return count > 0;
    }

    /// <summary>
    /// Waits for an order to move from "In Preparation" to "Finished Orders" section.
    /// Use this to verify that an order has been completed by the kitchen.
    /// </summary>
    /// <param name="orderNumber">The order number to wait for</param>
    /// <param name="timeoutMs">Maximum time to wait in milliseconds (default: 30 seconds)</param>
    /// <returns>True if order moves to finished, false if timeout</returns>
    public async Task<bool> WaitForOrderToFinishAsync(string orderNumber, int timeoutMs = 30000)
    {
        try
        {
            var orderTestId = $"finished-order-{orderNumber.ToLower()}";
            await Page.GetByTestId(orderTestId).WaitForAsync(new() { Timeout = timeoutMs });
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets order details from the "In Preparation" section.
    /// </summary>
    /// <param name="orderNumber">The order number</param>
    /// <returns>CustomerOrder object or null if not found</returns>
    public async Task<CustomerOrder?> GetOrderInPreparationAsync(string orderNumber)
    {
        var orderTestId = $"preparation-order-{orderNumber.ToLower()}";
        var orderCard = Page.GetByTestId(orderTestId);

        if (await orderCard.CountAsync() == 0)
        {
            return null;
        }

        return new CustomerOrder
        {
            OrderNumber = orderNumber,
            Items = new List<CustomerOrderItem>()
        };
    }

    /// <summary>
    /// Gets order details from the "Finished Orders" section.
    /// </summary>
    /// <param name="orderNumber">The order number</param>
    /// <returns>CustomerOrder object or null if not found</returns>
    public async Task<CustomerOrder?> GetFinishedOrderAsync(string orderNumber)
    {
        var orderTestId = $"finished-order-{orderNumber.ToLower()}";
        var orderCard = Page.GetByTestId(orderTestId);

        if (await orderCard.CountAsync() == 0)
        {
            return null;
        }

        return new CustomerOrder
        {
            OrderNumber = orderNumber,
            Items = new List<CustomerOrderItem>()
        };
    }

    #region Internal Helper Methods

    private async Task<CustomerOrder> ParseOrderCardAsync(ILocator orderCard, string orderNumber)
    {
        var order = new CustomerOrder
        {
            OrderNumber = orderNumber,
            Items = new List<CustomerOrderItem>()
        };

        // Customer order status currently doesn't display individual items
        // Only the order reference is shown
        
        return order;
    }

    #endregion
}

public class CustomerOrder
{
    public string OrderNumber { get; set; } = string.Empty;
    public List<CustomerOrderItem> Items { get; set; } = new();
}

public class CustomerOrderItem
{
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
