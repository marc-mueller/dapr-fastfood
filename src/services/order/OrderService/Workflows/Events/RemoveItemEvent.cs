namespace OrderPlacement.Workflows.Events;

public class RemoveItemEvent
{
    public static string Name => nameof(RemoveItemEvent);
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
}