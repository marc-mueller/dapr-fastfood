namespace FastFood.FeatureManagement.Common.Services;

/// <summary>
/// Feature manager that integrates with OpenTelemetry for observability.
/// Wraps IFeatureManager to add activity tags and metrics.
/// </summary>
public interface IObservableFeatureManager
{
    /// <summary>
    /// Checks whether a feature is enabled and records telemetry.
    /// </summary>
    /// <param name="feature">The name of the feature to check.</param>
    /// <returns>True if the feature is enabled; otherwise, false.</returns>
    Task<bool> IsEnabledAsync(string feature);

    /// <summary>
    /// Checks whether a feature is enabled for a specific context and records telemetry.
    /// </summary>
    /// <typeparam name="TContext">The type of the context to evaluate the feature against.</typeparam>
    /// <param name="feature">The name of the feature to check.</param>
    /// <param name="context">The context to evaluate the feature against.</param>
    /// <returns>True if the feature is enabled; otherwise, false.</returns>
    Task<bool> IsEnabledAsync<TContext>(string feature, TContext context);
}
