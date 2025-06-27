namespace OrderService.Common.Dtos;

public enum OrderDtoState
{
    Creating,
    Confirmed,
    Paid,
    Processing,
    Prepared,
    Delivering,
    Closed,
}