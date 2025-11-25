namespace FinanceService.Entities;

public enum OrderState
{
    Created = 0,
    PaymentPending = 1,
    Paid = 2,
    InPreparation = 3,
    ReadyForPickup = 4,
    Delivered = 5,
    Cancelled = 6,
    Closed = 7
}
