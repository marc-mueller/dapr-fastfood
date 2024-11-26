namespace KitchenService.Entities;

public class KitchenOrder
{
    public KitchenOrder()
    {
            Items = new List<KitchenOrderItem>();
    }
    public Guid Id { get; set; }
    public string? OrderReference { get; set; }
    public ICollection<KitchenOrderItem> Items { get; set; }
}