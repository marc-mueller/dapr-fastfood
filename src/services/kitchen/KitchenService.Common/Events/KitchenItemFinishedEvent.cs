namespace KitchenService.Common.Events;

public class KitchenItemFinishedEvent
{
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
}