namespace OrderService.Common.Dtos;

public class ItemAcknowledgement
{
    public Guid OrderId { get; set; }
    public Guid ItemId { get; set; }
    public string Message { get; set; }
}