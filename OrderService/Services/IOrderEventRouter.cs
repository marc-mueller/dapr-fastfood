namespace OrderPlacement.Services;

public interface IOrderEventRouter
{
    Task RegisterOrderForService(Guid orderid, OrderEventRoutingTarget target);
    Task<OrderEventRoutingTarget> GetRoutingTargetForOrder(Guid orderid);
    Task RemoveRoutingTargetForOrder(Guid orderid);
}