namespace FinanceService.Entities;

public class Order
{
    public Guid Id { get; set; }
    public OrderType Type { get; set; }
    public OrderState State { get; set; }
    
    // Foreign key to Customer
    public Guid? CustomerId { get; set; }
    
    // Navigation property
    public Customer? Customer { get; set; }
    
    // Navigation property to OrderItems
    public List<OrderItem>? Items { get; set; }
    
    public decimal? ServiceFee { get; set; }
    public decimal? Discount { get; set; }
    public string? CustomerComments { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
}
