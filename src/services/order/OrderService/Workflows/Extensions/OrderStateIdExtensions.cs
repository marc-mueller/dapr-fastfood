namespace OrderPlacement.Workflows.Extensions;

public static class OrderStateIdExtensions
{
    public static string ToOrderStateId(this Guid orderId)
    {
        return $"WF-Order-{orderId}";
    }
}