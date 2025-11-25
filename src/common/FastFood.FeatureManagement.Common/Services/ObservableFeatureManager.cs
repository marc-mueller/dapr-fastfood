using System.Diagnostics;
using FastFood.Observability.Common;
using Microsoft.FeatureManagement;

namespace FastFood.FeatureManagement.Common.Services;

/// <summary>
/// Implementation of IObservableFeatureManager that wraps IFeatureManager
/// and adds OpenTelemetry activity tags and metrics.
/// </summary>
public class ObservableFeatureManager : IObservableFeatureManager
{
    private readonly IFeatureManager _featureManager;
    private readonly IObservability _observability;

    public ObservableFeatureManager(IFeatureManager featureManager, IObservability observability)
    {
        _featureManager = featureManager ?? throw new ArgumentNullException(nameof(featureManager));
        _observability = observability ?? throw new ArgumentNullException(nameof(observability));
    }

    /// <inheritdoc />
    public async Task<bool> IsEnabledAsync(string feature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(feature);

        var isEnabled = await _featureManager.IsEnabledAsync(feature);

        RecordFeatureEvaluation(feature, isEnabled);

        return isEnabled;
    }

    /// <inheritdoc />
    public async Task<bool> IsEnabledAsync<TContext>(string feature, TContext context)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(feature);

        var isEnabled = await _featureManager.IsEnabledAsync(feature, context);

        RecordFeatureEvaluation(feature, isEnabled);

        return isEnabled;
    }

    private void RecordFeatureEvaluation(string feature, bool isEnabled)
    {
        // Add activity tag for distributed tracing
        var activity = Activity.Current;
        if (activity != null)
        {
            activity.SetTag($"feature.{feature}.enabled", isEnabled);
        }

        // Emit metric for feature evaluation
        _observability.FeatureEvaluationCounter.Add(1,
            new KeyValuePair<string, object?>("feature", feature),
            new KeyValuePair<string, object?>("enabled", isEnabled));
    }
}
