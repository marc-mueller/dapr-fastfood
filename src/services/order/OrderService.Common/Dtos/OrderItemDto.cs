namespace OrderService.Common.Dtos;

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public Decimal ItemPrice { get; set; }
    public string? ProductDescription { get; set; }
    public string? CustomerComments { get; set; }
    public OrderItemDtoState State { get; set; }
}