namespace OrderPlacement.Workflows.Events;

public class StartProcessingEvent
{
    public static string Name => nameof(StartProcessingEvent);
    public Guid OrderId { get; set; }
}