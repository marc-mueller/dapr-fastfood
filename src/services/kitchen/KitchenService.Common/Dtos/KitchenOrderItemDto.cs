namespace KitchenService.Common.Dtos;

public class KitchenOrderItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string? ProductDescription { get; set; }
    public string? CustomerComments { get; set; }
    public KitchenOrderItemDtoState State { get; set; }
    public Guid OrderId { get; set; }
}