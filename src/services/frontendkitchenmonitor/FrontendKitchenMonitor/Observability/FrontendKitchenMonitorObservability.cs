using FastFood.Observability.Common;

namespace FinanceService.Observability;

internal class FrontendKitchenMonitorObservability : ObservabilityBase, IFrontendKitchenMonitorObservability
{
    internal FrontendKitchenMonitorObservability(string serviceName, string activitySourceName) : base(serviceName, activitySourceName, typeof(FrontendKitchenMonitorObservability).Assembly.GetName().Version?.ToString())
    {
    }
}