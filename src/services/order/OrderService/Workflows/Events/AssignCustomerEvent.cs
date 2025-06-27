using OrderService.Models.Entities;

namespace OrderPlacement.Workflows.Events;

public class AssignCustomerEvent
{
    public static string Name => nameof(AssignCustomerEvent);
    public Guid OrderId { get; set; }
    public Customer Customer { get; set; }
}