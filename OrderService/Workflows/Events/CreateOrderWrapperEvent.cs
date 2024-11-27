namespace OrderPlacement.Workflows.Events;

public class CreateOrderWrapperEvent
{
    public CreateOrderWrapperEvent()
    {
    }
    public CreateOrderWrapperEvent(AssignCustomerEvent subEvent)
    {
        AssignCustomerEvent = subEvent;
        SubEventType = CreateOrderSubEventType.AssignCustomerEvent;
    }
    
    public CreateOrderWrapperEvent(AssignInvoiceAddressEvent subEvent)
    {
        AssignInvoiceAddressEvent = subEvent;
        SubEventType = CreateOrderSubEventType.AssignInvoiceAddressEvent;
    }
    
    public CreateOrderWrapperEvent(AssignDeliveryAddressEvent subEvent)
    {
        AssignDeliveryAddressEvent = subEvent;
        SubEventType = CreateOrderSubEventType.AssignDeliveryAddressEvent;
    }
    
    public CreateOrderWrapperEvent(AddItemEvent subEvent)
    {
        AddItemEvent = subEvent;
        SubEventType = CreateOrderSubEventType.AddItemEvent;
    }
    
    public CreateOrderWrapperEvent(RemoveItemEvent subEvent)
    {
        RemoveItemEvent = subEvent;
        SubEventType = CreateOrderSubEventType.RemoveItemEvent;
    }
    
    public CreateOrderWrapperEvent(ConfirmOrderEvent subEvent)
    {
        ConfirmOrderEvent = subEvent;
        SubEventType = CreateOrderSubEventType.ConfirmOrderEvent;
    }
    
    public static string Name => nameof(CreateOrderWrapperEvent);
    public Guid OrderId { get; set; }
    
    public CreateOrderSubEventType SubEventType { get; set; }

    public AssignCustomerEvent? AssignCustomerEvent { get; set; }
    public AssignInvoiceAddressEvent? AssignInvoiceAddressEvent { get; set; }
    public AssignDeliveryAddressEvent? AssignDeliveryAddressEvent { get; set; }
    public AddItemEvent? AddItemEvent { get; set; }
    public RemoveItemEvent? RemoveItemEvent { get; set; }
    public ConfirmOrderEvent? ConfirmOrderEvent { get; set; }

    public enum CreateOrderSubEventType
    {
        AssignCustomerEvent,
        AssignInvoiceAddressEvent,
        AssignDeliveryAddressEvent,
        AddItemEvent,
        RemoveItemEvent,
        ConfirmOrderEvent
    }
}