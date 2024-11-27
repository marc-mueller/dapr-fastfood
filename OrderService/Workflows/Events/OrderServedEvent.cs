namespace OrderPlacement.Workflows.Events;

public class OrderServedEvent
{
    public static string Name => nameof(OrderServedEvent);
    public Guid OrderId { get; set; }
}