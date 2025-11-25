namespace FinanceService.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal ItemPrice { get; set; }
    public string? ProductDescription { get; set; }
    public string? CustomerComments { get; set; }
    public OrderItemState State { get; set; }
    
    // Foreign key to Order
    public Guid OrderId { get; set; }
    
    // Navigation property
    public Order? Order { get; set; }
}
