namespace OrderPlacement.Workflows.Events;

public class ConfirmOrderEvent
{
    public static string Name => nameof(ConfirmOrderEvent);
    public Guid OrderId { get; set; }
}