namespace OrderService.Models.Entities;

public class Order
{
    public Order()
    {
        Items = new List<OrderItem>();
    }
    public Guid Id { get; set; }
    
    public string? OrderReference { get; set; }

    public OrderType Type { get; set; }
    public OrderState State { get; set; }

    public Customer? Customer { get; set; }

    public ICollection<OrderItem>? Items { get; set; }
    
    public string? CustomerComments { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset PaidAt { get; set; }
    public DateTimeOffset StartProcessingAt { get; set; }
    public DateTimeOffset DeliveredAt { get; set; }
    public DateTimeOffset ClosedAt { get; set; }
    public DateTimeOffset StartDeliveringAt { get; set; }
    public DateTimeOffset PreparationFinishedAt { get; set; }
}