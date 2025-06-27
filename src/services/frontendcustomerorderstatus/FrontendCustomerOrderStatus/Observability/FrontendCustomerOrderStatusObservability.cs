using FastFood.Observability.Common;

namespace FinanceService.Observability;

internal class FrontendCustomerOrderStatusObservability : ObservabilityBase, IFrontendCustomerOrderStatusObservability
{
    internal FrontendCustomerOrderStatusObservability(string serviceName, string activitySourceName) : base(serviceName, activitySourceName, typeof(FrontendCustomerOrderStatusObservability).Assembly.GetName().Version?.ToString())
    {
    }
}