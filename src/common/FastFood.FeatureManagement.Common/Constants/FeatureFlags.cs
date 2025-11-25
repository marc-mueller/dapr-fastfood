namespace FastFood.FeatureManagement.Common.Constants;

/// <summary>
/// Central repository for all feature flag names used across the FastFood application.
/// </summary>
public static class FeatureFlags
{
    /// <summary>
    /// Feature 1: Show/hide loyalty card scanning UI and apply discount.
    /// Services: FrontendSelfServicePos, OrderService
    /// </summary>
    public const string LoyaltyProgram = "LoyaltyProgram";

    /// <summary>
    /// Feature 2: A/B test between single-page vs. multi-step checkout.
    /// Services: FrontendSelfServicePos
    /// </summary>
    public const string NewCheckoutExperience = "NewCheckoutExperience";

    /// <summary>
    /// Feature 3: Toggle dark/light theme in frontend.
    /// Services: FrontendSelfServicePos
    /// </summary>
    public const string DarkMode = "DarkMode";

    /// <summary>
    /// Feature 5: Route orders to Workflow-based implementation instead of Actor/State.
    /// Services: OrderService
    /// </summary>
    public const string UseWorkflowImplementation = "UseWorkflowImplementation";

    /// <summary>
    /// Feature 6: Apply surge pricing during peak hours.
    /// Services: OrderService
    /// </summary>
    public const string DynamicPricing = "DynamicPricing";

    /// <summary>
    /// Feature 8: Automatically prioritize orders in kitchen based on prep time.
    /// Services: KitchenService
    /// </summary>
    public const string AutoPrioritization = "AutoPrioritization";
}
