namespace OrderPlacement.Workflows.Events;

public class ItemFinishedEvent
{
    public static string Name => nameof(ItemFinishedEvent);
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
    
}