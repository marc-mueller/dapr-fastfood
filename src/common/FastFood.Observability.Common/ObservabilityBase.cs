using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FastFood.Observability.Common;

public class ObservabilityBase : IObservability
{

  protected ObservabilityBase(string serviceName, string activitySourceName, string? version = null)
  {
    this.ServiceName = serviceName;
    this.Meter = new Meter(this.ServiceName, version);
    this.ActivitySourceName = activitySourceName;
    this.ActivitySource = new ActivitySource(activitySourceName, version);
    
    // Initialize feature flag metrics
    this.FeatureEvaluationCounter = this.Meter.CreateCounter<long>(
        "feature.evaluation",
        description: "Number of feature flag evaluations");
    
    this.FeatureUsageCounter = this.Meter.CreateCounter<long>(
        "feature.usage",
        description: "Number of times features are actively used");
  }

  public string ServiceName { get; }
  
  public Counter<long> FeatureEvaluationCounter { get; }
  
  public Counter<long> FeatureUsageCounter { get; }

  protected Meter Meter { get; }

  public string ActivitySourceName { get; }

  public virtual Activity? StartActivity(string name = "", ActivityKind kind = ActivityKind.Internal)
  {
    return this.ActivitySource?.StartActivity(name, kind);
  }

  public virtual Activity? StartActivity(
    Type? callerType,
    string name = "",
    ActivityKind kind = ActivityKind.Internal,
    bool includeCallerTypeInName = false)
  {
    if (callerType != (Type) null && this.StartActivityExclusionPredicate(callerType))
      return (Activity) null;
    if (!includeCallerTypeInName || callerType == (Type) null)
      return this.ActivitySource?.StartActivity(name, kind);
    ActivitySource activitySource = this.ActivitySource;
    if (activitySource == null)
      return (Activity) null;
    string name1;
    if (!callerType.IsGenericType)
      name1 = $"{callerType.Name}.{name}";
    else
      name1 = $"{callerType.Name.AsSpan(0, callerType.Name.IndexOf("`", StringComparison.InvariantCulture)).ToString()}<{string.Join(",", ((IEnumerable<Type>) callerType.GenericTypeArguments).Select<Type, string>((Func<Type, string>) (t => t.Name)))}>.{name}";
    int kind1 = (int) kind;
    return activitySource.StartActivity(name1, (ActivityKind) kind1);
  }

  protected virtual Func<Type, bool> StartActivityExclusionPredicate { get; } = (Func<Type, bool>) (_ => false);

  protected virtual bool EnableDatabaseMetrics { get; } = true;

  protected virtual ActivitySource ActivitySource { get; }
}