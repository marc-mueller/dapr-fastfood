using System.Diagnostics.Metrics;
using FastFood.Observability.Common;

namespace FinanceService.Observability;

public interface IKitchenServiceObservability : IObservability
{
    public Histogram<double> OrderItemPreparationDuration { get; }
    
    public Histogram<double> OrderPerparationDuration { get;  }

    public Histogram<long> OrderItemsCount { get;  }

    public Counter<long> OrdersCounter { get;  }
}