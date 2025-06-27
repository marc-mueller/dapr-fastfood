namespace OrderPlacement.Workflows.Events;

public class ConfirmPaymentEvent
{
    public static string Name => nameof(ConfirmPaymentEvent);
    public Guid OrderId { get; set; }
}