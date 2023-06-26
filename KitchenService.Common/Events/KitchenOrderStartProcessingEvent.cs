namespace KitchenService.Common.Events;

public class KitchenOrderStartProcessingEvent
{
    public Guid OrderId { get; set; }
}