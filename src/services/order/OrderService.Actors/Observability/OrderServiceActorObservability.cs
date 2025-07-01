using System.Diagnostics.Metrics;
using FastFood.Observability.Common;

namespace FinanceService.Observability;

internal class OrderServiceActorObservability : ObservabilityBase, IOrderServiceActorObservability
{
    internal OrderServiceActorObservability(string serviceName, string activitySourceName) : base(serviceName, activitySourceName, typeof(OrderServiceActorObservability).Assembly.GetName().Version?.ToString())
    {
        OrdersCreatedCounter = Meter.CreateCounter<long>("orderservice_orders_created", description: "Number of orders created");
        OrdersPaidCounter = Meter.CreateCounter<long>( "orderservice_orders_paid", description: "Number of orders paid");
        OrdersClosedCounter = Meter.CreateCounter<long>("orderservice_orders_closed", description: "Number of orders served or closed (delivered or served)");
        OrderItemsCount = Meter.CreateHistogram<long>("orderservice_orders_item_count", unit: "items", description: "Number of items per order");
        OrderTotalAmount = Meter.CreateHistogram<decimal>( "orderservice_orders_total_amount", unit: "currency", description: "Total amount of the order in currency units");
        OrderSalesDuration = Meter.CreateHistogram<double>("orderservice_orders_sales_duration_milliseconds", unit: "ms", description: "Duration from creation to payment confirmation");
        OrderPerparationDuration = Meter.CreateHistogram<double>("orderservice_orders_preparation_duration_milliseconds", unit: "ms", description: "Time taken to prepare the entire order");
        OrderDeliveryDuration = Meter.CreateHistogram<double>("orderservice_orders_delivery_duration_milliseconds", unit: "ms", description: "Time taken to deliver the order");
        OrderTotalDuration = Meter.CreateHistogram<double>("orderservice_orders_total_duration_milliseconds", unit: "ms", description: "Total time from creation to closed/completed");
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