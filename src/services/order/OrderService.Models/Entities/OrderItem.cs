namespace OrderService.Models.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Decimal ItemPrice { get; set; }
    public string? ProductDescription { get; set; }
    public string? CustomerComments { get; set; }
    public OrderItemState State { get; set; }
}