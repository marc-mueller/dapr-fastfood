
using OrderService.Models.Entities;

namespace OrderPlacement.Storages;

public interface IOrderStorage
{
    Task<IEnumerable<Order>> GetOrders();
    Task<Order> GetOrderById(Guid id);
    Task<IEnumerable<Order>> GetActiveOrders();
    Task<Order> UpdateOder(Order order);
    
}