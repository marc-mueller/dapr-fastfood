using System.Diagnostics.Metrics;
using FastFood.Observability.Common;

namespace FinanceService.Observability;

internal class OrderServiceActorObservability : ObservabilityBase, IOrderServiceActorObservability
{
    internal OrderServiceActorObservability(string serviceName, string activitySourceName) : base(serviceName, activitySourceName, typeof(OrderServiceActorObservability).Assembly.GetName().Version?.ToString())
    {
        OrdersCreatedCounter = Meter.CreateCounter<long>("orderservice.orders.created", description: "Number of orders created");
        OrdersPaidCounter = Meter.CreateCounter<long>( "orderservice.orders.paid", description: "Number of orders paid");
        OrdersClosedCounter = Meter.CreateCounter<long>("orderservice.orders.closed", description: "Number of orders served or closed (delivered or served)");
        OrderItemsCount = Meter.CreateHistogram<long>("orderservice.orders.item_count", unit: "items", description: "Number of items per order");
        OrderTotalAmount = Meter.CreateHistogram<decimal>( "orderservice.orders.total_amount", unit: "currency", description: "Total amount of the order in currency units");
        OrderSalesDuration = Meter.CreateHistogram<double>("orderservice.orders.sales.duration_milliseconds", unit: "ms", description: "Duration from creation to payment confirmation");
        OrderPerparationDuration = Meter.CreateHistogram<double>("orderservice.orders.preparation.duration_milliseconds", unit: "ms", description: "Time taken to prepare the entire order");
        OrderDeliveryDuration = Meter.CreateHistogram<double>("orderservice.orders.delivery.duration_milliseconds", unit: "ms", description: "Time taken to deliver the order");
        OrderTotalDuration = Meter.CreateHistogram<double>("orderservice.orders.total.duration_milliseconds", unit: "ms", description: "Total time from creation to closed/completed");
    }
    
    public Histogram<decimal> OrderTotalAmount { get; }

    public Histogram<double> OrderTotalDuration { get;  }

    public Histogram<double> OrderDeliveryDuration { get;  }

    public Histogram<double> OrderPerparationDuration { get;  }

    public Histogram<long> OrderItemsCount { get;  }

    public Counter<long> OrdersClosedCounter { get;  }

    public Counter<long> OrdersPaidCounter { get;  }

    public Counter<long> OrdersCreatedCounter { get;  }
    
    public Histogram<double> OrderSalesDuration { get;  }
}