namespace OrderService.Models.Entities;

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