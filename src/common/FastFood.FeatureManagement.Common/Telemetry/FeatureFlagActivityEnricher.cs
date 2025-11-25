using System.Diagnostics;

namespace FastFood.FeatureManagement.Common.Telemetry;

/// <summary>
/// Helper methods for enriching activities with feature flag information.
/// </summary>
public static class FeatureFlagActivityEnricher
{
    /// <summary>
    /// Records that a feature was actively used (not just evaluated).
    /// Adds a tag to indicate actual feature usage occurred.
    /// </summary>
    /// <param name="activity">The current activity.</param>
    /// <param name="featureName">The name of the feature that was used.</param>
    /// <param name="action">The action taken (e.g., "discount_applied", "queue_reordered").</param>
    public static void RecordFeatureUsage(Activity? activity, string featureName, string action)
    {
        if (activity == null) return;

        activity.SetTag($"feature.{featureName}.used", true);
        activity.SetTag($"feature.{featureName}.action", action);
    }

    /// <summary>
    /// Records a feature-specific metric value.
    /// </summary>
    /// <param name="activity">The current activity.</param>
    /// <param name="featureName">The name of the feature.</param>
    /// <param name="metricName">The metric name (e.g., "discount_amount", "surge_multiplier").</param>
    /// <param name="value">The metric value.</param>
    public static void RecordFeatureMetric(Activity? activity, string featureName, string metricName, object value)
    {
        if (activity == null) return;

        activity.SetTag($"feature.{featureName}.{metricName}", value);
    }
}
