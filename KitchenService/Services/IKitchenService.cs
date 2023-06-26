using KitchenService.Entities;

namespace KitchenService.Services;

public interface IKitchenService
{
    Task<KitchenOrder> AddOrder(Guid orderId, IEnumerable<Tuple<Guid, Guid, int, string?>> items);
    Task<IEnumerable<KitchenOrder>> GetPendingOrders();
    Task<IEnumerable<KitchenOrderItem>> GetPendingItems();
    Task<KitchenOrderItem> SetItemAsFinished(Guid id);
}