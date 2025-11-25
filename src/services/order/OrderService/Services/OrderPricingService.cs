using System.Diagnostics;
using FastFood.FeatureManagement.Common.Constants;
using FastFood.FeatureManagement.Common.Services;
using FastFood.FeatureManagement.Common.Telemetry;
using FastFood.Observability.Common;
using Microsoft.FeatureManagement.FeatureFilters;
using OrderService.Models.Entities;

namespace OrderPlacement.Services;

/// <summary>
/// Pricing breakdown for an order including base price, fees, and discounts.
/// </summary>
public class OrderPricingBreakdown
{
    public decimal Subtotal { get; set; }
    public decimal? ServiceFee { get; set; }
    public decimal? Discount { get; set; }
    public decimal Total { get; set; }
}

/// <summary>
/// Service for calculating order prices with feature-based dynamic pricing.
/// </summary>
public interface IOrderPricingService
{
    /// <summary>
    /// Calculates the pricing breakdown for an order, applying fees and discounts based on feature flags.
    /// </summary>
    Task<OrderPricingBreakdown> CalculateOrderPricing(Order order);
    
    /// <summary>
    /// Calculates the total price for an order (backward compatibility).
    /// </summary>
    Task<decimal> CalculateOrderTotal(Order order);
}

public class OrderPricingService : IOrderPricingService
{
    private readonly IObservableFeatureManager _featureManager;
    private readonly IObservability _observability;
    private const decimal SurgeMultiplier = 1.2m;

    public OrderPricingService(IObservableFeatureManager featureManager, IObservability observability)
    {
        _featureManager = featureManager;
        _observability = observability;
    }

    public async Task<OrderPricingBreakdown> CalculateOrderPricing(Order order)
    {
        var breakdown = new OrderPricingBreakdown();
        
        if (order.Items == null || !order.Items.Any())
        {
            breakdown.Subtotal = 0m;
            breakdown.Total = 0m;
            return breakdown;
        }

        // Calculate subtotal from items
        breakdown.Subtotal = order.Items.Sum(item => item.ItemPrice * item.Quantity);

        // Create targeting context for per-order feature evaluation
        var targetingContext = new TargetingContext
        {
            UserId = order.Id.ToString(),
            Groups = new List<string>()
        };

        // Check if dynamic pricing feature is enabled for this specific order
        var dynamicPricingEnabled = await _featureManager.IsEnabledAsync(
            FeatureFlags.DynamicPricing, 
            targetingContext
        );
        
        if (dynamicPricingEnabled)
        {
            // Calculate peak hour service fee (20% of subtotal)
            breakdown.ServiceFee = Math.Round(breakdown.Subtotal * (SurgeMultiplier - 1m), 2);
            
            // Record feature usage in telemetry
            var activity = Activity.Current;
            FeatureFlagActivityEnricher.RecordFeatureUsage(activity, FeatureFlags.DynamicPricing, "service_fee_applied");
            FeatureFlagActivityEnricher.RecordFeatureMetric(activity, FeatureFlags.DynamicPricing, "service_fee", breakdown.ServiceFee.Value);
            FeatureFlagActivityEnricher.RecordFeatureMetric(activity, FeatureFlags.DynamicPricing, "subtotal", breakdown.Subtotal);
            
            _observability.FeatureUsageCounter.Add(1,
                new KeyValuePair<string, object?>("feature", FeatureFlags.DynamicPricing),
                new KeyValuePair<string, object?>("action", "service_fee_applied"));
        }

        // Calculate total
        breakdown.Total = breakdown.Subtotal + (breakdown.ServiceFee ?? 0m);
        
        return breakdown;
    }

    public async Task<decimal> CalculateOrderTotal(Order order)
    {
        var breakdown = await CalculateOrderPricing(order);
        return breakdown.Total;
    }
}
