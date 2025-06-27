namespace OrderService.Common.Dtos;

public class OrderAcknowledgement
{
    public Guid OrderId { get; set; }
    public string Message { get; set; }
}