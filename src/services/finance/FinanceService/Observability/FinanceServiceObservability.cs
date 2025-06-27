using FastFood.Observability.Common;

namespace FinanceService.Observability;

internal class FinanceServiceObservability : ObservabilityBase, IFinanceServiceObservability
{
    internal FinanceServiceObservability(string serviceName, string activitySourceName) : base(serviceName, activitySourceName, typeof(FinanceServiceObservability).Assembly.GetName().Version?.ToString())
    {
    }
}