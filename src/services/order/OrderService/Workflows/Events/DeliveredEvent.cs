namespace OrderPlacement.Workflows.Events;

public class DeliveredEvent
{
    public static string Name => nameof(DeliveredEvent);
    public Guid OrderId { get; set; }
}