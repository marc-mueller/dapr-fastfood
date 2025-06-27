namespace OrderPlacement.Workflows.Events;

public class StartDeliveryEvent
{
    public static string Name => nameof(StartDeliveryEvent);
    public Guid OrderId { get; set; }
}