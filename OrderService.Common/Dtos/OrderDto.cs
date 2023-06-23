namespace OrderService.Common.Dtos;

public class OrderDto
{
    public OrderDto()
    {
        Items = new List<OrderItem>();
    }
    public Guid Id { get; set; }

    public OrderType Type { get; set; }
    public OrderState State { get; set; }

    public CustomerDto Customer { get; set; }

    public ICollection<OrderItem> Items { get; set; }
    
    public string? CustomerComments { get; set; }
    
}