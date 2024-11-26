using KitchenService.Entities;

namespace KitchenService.Services;

public interface IKitchenService
{
    Task<KitchenOrder> AddOrder(Guid orderId, string orderReference, IEnumerable<Tuple<Guid, Guid, string, int, string?>> items);
    Task<IEnumerable<KitchenOrder>> GetPendingOrders();
    Task<KitchenOrder?> GetPendingOrder(Guid id);
    Task<IEnumerable<KitchenOrderItem>> GetPendingItems();
    Task<KitchenOrderItem> SetItemAsFinished(Guid id);
}