using OrderService.Models.Entities;

namespace OrderPlacement.Workflows.Events;

public class AddItemEvent
{
    public static string Name => nameof(AddItemEvent);
    public Guid OrderId { get; set; }
    public OrderItem Item { get; set; }
}