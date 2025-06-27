using System.Diagnostics.Metrics;
using FastFood.Observability.Common;

namespace FinanceService.Observability;

internal class KitchenServiceObservability : ObservabilityBase, IKitchenServiceObservability
{
    internal KitchenServiceObservability(string serviceName, string activitySourceName) : base(serviceName, activitySourceName, typeof(KitchenServiceObservability).Assembly.GetName().Version?.ToString())
    {
        OrdersCounter = Meter.CreateCounter<long>("kitchenservice.orders.created", description: "Number of orders created");
        OrderItemsCount = Meter.CreateHistogram<long>("kitchenservice.orders.item_count", unit: "items", description: "Number of items per order");
        OrderPerparationDuration = Meter.CreateHistogram<double>("kitchenservice.orders.preparation.duration_milliseconds", unit: "s", description: "Time taken to prepare the entire order");
        OrderItemPreparationDuration = Meter.CreateHistogram<double>("kitchenservice.orders.item.preparation.duration_milliseconds", unit: "s", description: "Time taken to prepare each item in the order");
    }

    public Histogram<double> OrderItemPreparationDuration { get; }

    public Histogram<double> OrderPerparationDuration { get;  }

    public Histogram<long> OrderItemsCount { get;  }

    public Counter<long> OrdersCounter { get;  }
}