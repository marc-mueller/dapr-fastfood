using System.Diagnostics.Metrics;
using FastFood.Observability.Common;

namespace FinanceService.Observability;

public interface IOrderServiceObservability : IObservability
{
    public Histogram<decimal> OrderTotalAmount { get;  }

    public Histogram<double> OrderTotalDuration { get;  }

    public Histogram<double> OrderDeliveryDuration { get;  }

    public Histogram<double> OrderPerparationDuration { get;  }

    public Histogram<long> OrderItemsCount { get;  }

    public Counter<long> OrdersClosedCounter { get;  }

    public Counter<long> OrdersPaidCounter { get;  }

    public Counter<long> OrdersCreatedCounter { get;  }
    
    public Histogram<double> OrderSalesDuration { get;  }
    
    public Counter<long> ClientChannelOrdersCreatedCounter { get; }
}