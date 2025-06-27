using System.Diagnostics;
using System.Diagnostics.Metrics;
using FinanceService.Observability;

namespace FinanceService.Unit.Tests.Helpers;

public class TestFinanceServiceObservability : IFinanceServiceObservability
{
    public TestFinanceServiceObservability()
    {
        ServiceName = "FinanceService";
        ActivitySourceName = "FinanceService";
    }
    
    public string ServiceName { get; }
    public string ActivitySourceName { get; }
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