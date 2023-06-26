using OrderService.Models.Entities;

namespace OrderPlacement.Services;

public interface IOrderProcessingService
{
    public Task<Order> GetOrder(Guid orderid);
    // public Task<IEnumerable<Order>> GetOrders();
    // public Task<IEnumerable<Order>> GetActiveOrders();

    public Task<Order> CreateOrder(Order order);
    public Task<Order> AssignCustomer(Guid orderid, Customer customer);
    public Task<Order> AssignInvoiceAddress(Guid orderid, Address address);
    public Task<Order> AssignDeliveryAddress(Guid orderid, Address address);
    public Task<Order> AddItem(Guid orderid, OrderItem item);
    public Task<Order> RemoveItem(Guid orderid, Guid itemId);
    public Task<Order> ConfirmOrder(Guid orderid);
    public Task<Order> ConfirmPayment(Guid orderid);
    public Task<Order> StartProcessing(Guid orderid);
    public Task<Order> FinishedItem(Guid orderid, Guid itemId);
    public Task<Order> Served(Guid orderid);
    public Task<Order> StartDelivery(Guid orderid);
    public Task<Order> Delivered(Guid orderid);

    
}