namespace KitchenService.Entities;

public class KitchenOrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? ProductDescription { get; set; }
    public string? CustomerComments { get; set; }
    public KitchenOrderItemState State { get; set; }
    public Guid OrderId { get; set; }
}