namespace FinanceService.Common.Dtos;

public class OrderDto
{
    public OrderDto()
    {
        Items = new List<OrderItemDto>();
    }
    public Guid Id { get; set; }

    public OrderDtoType Type { get; set; }
    public OrderDtoState State { get; set; }

    public CustomerDto? Customer { get; set; }

    public ICollection<OrderItemDto>? Items { get; set; }
    
    /// <summary>
    /// Peak hour service fee applied to order (DynamicPricing feature flag).
    /// Null if no service fee applied.
    /// </summary>
    public decimal? ServiceFee { get; set; }
    
    /// <summary>
    /// Loyalty program discount applied to order (LoyaltyProgram feature flag).
    /// Null if no discount applied.
    /// </summary>
    public decimal? Discount { get; set; }
    
    public string? CustomerComments { get; set; }
    
}