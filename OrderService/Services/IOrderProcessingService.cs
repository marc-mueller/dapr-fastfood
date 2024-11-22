using OrderService.Models.Entities;

namespace OrderPlacement.Services;

public interface IOrderProcessingService
{
    // Queries
    public Task<Order> GetOrder(Guid orderid);
        
    // Commands
    public Task CreateOrder(Order order);
    public Task AssignCustomer(Guid orderid, Customer customer);
    public Task AssignInvoiceAddress(Guid orderid, Address address);
    public Task AssignDeliveryAddress(Guid orderid, Address address);
    public Task AddItem(Guid orderid, OrderItem item);
    public Task RemoveItem(Guid orderid, Guid itemId);
    public Task ConfirmOrder(Guid orderid);
    public Task ConfirmPayment(Guid orderid);
    public Task StartProcessing(Guid orderid);
    public Task FinishedItem(Guid orderid, Guid itemId);
    public Task Served(Guid orderid);
    public Task StartDelivery(Guid orderid);
    public Task Delivered(Guid orderid);
}

public interface IOrderProcessingServiceState : IOrderProcessingService { }

public interface IOrderProcessingServiceActor : IOrderProcessingService { }

public interface IOrderProcessingServiceWorkflow : IOrderProcessingService { }