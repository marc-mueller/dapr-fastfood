using System.Diagnostics;
using System.Diagnostics.Metrics;
using FinanceService.Observability;

namespace FinanceService.Unit.Tests.Helpers;

public class TestFinanceServiceObservability : IFinanceServiceObservability
{
    private readonly Meter _meter;
    
    public TestFinanceServiceObservability()
    {
        ServiceName = "FinanceService";
        ActivitySourceName = "FinanceService";
        _meter = new Meter(ServiceName);
        FeatureEvaluationCounter = _meter.CreateCounter<long>("feature.evaluation");
        FeatureUsageCounter = _meter.CreateCounter<long>("feature.usage");
    }
    
    public string ServiceName { get; }
    public string ActivitySourceName { get; }
    public Counter<long> FeatureEvaluationCounter { get; }
    public Counter<long> FeatureUsageCounter { get; }
    
    public Activity? StartActivity(string name = "", ActivityKind kind = ActivityKind.Internal)
    {
        return null;
    }

    public Activity? StartActivity(Type callerType, string name = "", ActivityKind kind = ActivityKind.Internal,
        bool includeCallerTypeInName = false)
    {
        return null;
    }
}