using OrderService.Models.Entities;

namespace OrderPlacement.Workflows.Events;

public class AssignDeliveryAddressEvent
{
    public static string Name => nameof(AssignDeliveryAddressEvent);
    public Guid OrderId { get; set; }
    public Address Address { get; set; }
}