using FastFood.Observability.Common;

namespace FinanceService.Observability;

internal class FrontendSelfServicePosObservability : ObservabilityBase, IFrontendSelfServicePosObservability
{
    internal FrontendSelfServicePosObservability(string serviceName, string activitySourceName) : base(serviceName, activitySourceName, typeof(FrontendSelfServicePosObservability).Assembly.GetName().Version?.ToString())
    {
    }
}