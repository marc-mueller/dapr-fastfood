using OrderService.Models.Entities;

namespace OrderPlacement.Workflows.Events;

public class AssignCustomerEvent
{
    public static string Name => nameof(AssignCustomerEvent);
    public Guid OrderId { get; set; }
    public Customer Customer { get; set; }
}

public class AssignInvoiceAddressEvent
{
    public static string Name => nameof(AssignInvoiceAddressEvent);
    public Guid OrderId { get; set; }
    public Address Address { get; set; }
}

public class AssignDeliveryAddressEvent
{
    public static string Name => nameof(AssignDeliveryAddressEvent);
    public Guid OrderId { get; set; }
    public Address Address { get; set; }
}

public class AddItemEvent
{
    public static string Name => nameof(AddItemEvent);
    public Guid OrderId { get; set; }
    public OrderItem Item { get; set; }
}

public class RemoveItemEvent
{
    public static string Name => nameof(RemoveItemEvent);
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
}

public class ConfirmOrderEvent
{
    public static string Name => nameof(ConfirmOrderEvent);
    public Guid OrderId { get; set; }
}

public class ConfirmPaymentEvent
{
    public static string Name => nameof(ConfirmPaymentEvent);
    public Guid OrderId { get; set; }
}

public class StartProcessingEvent
{
    public static string Name => nameof(StartProcessingEvent);
    public Guid OrderId { get; set; }
}

public class ItemFinishedEvent
{
    public static string Name => nameof(ItemFinishedEvent);
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
    
}

public class OrderServedEvent
{
    public static string Name => nameof(OrderServedEvent);
    public Guid OrderId { get; set; }
}

public class StartDeliveryEvent
{
    public static string Name => nameof(StartDeliveryEvent);
    public Guid OrderId { get; set; }
}

public class DeliveredEvent
{
    public static string Name => nameof(DeliveredEvent);
    public Guid OrderId { get; set; }
}