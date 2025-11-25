using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace FastFood.Observability.Common;

public interface IObservability
{
    string ServiceName { get; }

    string ActivitySourceName { get; }

    Activity? StartActivity([CallerMemberName] string name = "", ActivityKind kind = ActivityKind.Internal);

    Activity? StartActivity(
        Type callerType,
        [CallerMemberName] string name = "",
        ActivityKind kind = ActivityKind.Internal,
        bool includeCallerTypeInName = false);

    /// <summary>
    /// Counter for feature flag evaluations.
    /// </summary>
    Counter<long> FeatureEvaluationCounter { get; }

    /// <summary>
    /// Counter for feature flag actual usage (when feature is enabled and used).
    /// </summary>
    Counter<long> FeatureUsageCounter { get; }
}