namespace KitchenService.Common.Dtos;

public class KitchenOrderDto
{
    public KitchenOrderDto()
    {
        Items = new List<KitchenOrderItemDto>();
    }
    public Guid Id { get; set; }

    public string? OrderReference { get; set; }
    public ICollection<KitchenOrderItemDto> Items { get; set; }
}