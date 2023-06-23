namespace OrderService.Common.Dtos;

public enum OrderState
{
    Creating,
    Confirmed,
    Paid,
    Processing,
    Prepared,
    Delivering,
    Closed,
}