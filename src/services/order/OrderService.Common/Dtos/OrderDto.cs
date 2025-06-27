namespace OrderService.Common.Dtos;

public class OrderDto
{
    public OrderDto()
    {
        Items = new List<OrderItemDto>();
    }
    public Guid Id { get; set; }

    public string? OrderReference { get; set; }

    public OrderDtoType Type { get; set; }
    public OrderDtoState State { get; set; }

    public CustomerDto? Customer { get; set; }

    public ICollection<OrderItemDto>? Items { get; set; }
    
    public string? CustomerComments { get; set; }
    
}