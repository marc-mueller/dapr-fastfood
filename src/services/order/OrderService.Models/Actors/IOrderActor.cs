using Dapr.Actors;
using OrderService.Models.Entities;

namespace OrderService.Models.Actors;

public interface IOrderActor: IActor
{
    public Task<Order> CreateOrder(Order order);
    public Task<Order> AssignCustomer(Customer customer);
    public Task<Order> AssignInvoiceAddress(Address address);
    public Task<Order> AssignDeliveryAddress(Address address);
    public Task<Order> AddItem(OrderItem item);
    public Task<Order> RemoveItem(Guid itemId);
    public Task<Order> ConfirmOrder();
    public Task<Order> ConfirmPayment();
    public Task<Order> StartProcessing();
    public Task<Order> FinishedItem(Guid itemId);
    public Task<Order> Served();
    public Task<Order> StartDelivery();
    public Task<Order> Delivered();
    
    public Task<Order> GetOrder();
}