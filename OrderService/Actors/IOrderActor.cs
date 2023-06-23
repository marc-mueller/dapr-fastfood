using Dapr.Actors;
using OrderService.Common.Dtos;

namespace OrderPlacement.Actors;

public interface IOrderActor: IActor
{
    public Task<OrderDto> CreateOrder(OrderDto order);
    public Task<OrderDto> AssignCustomer(CustomerDto customer);
    public Task<OrderDto> AssignInvoiceAddress(Address address);
    public Task<OrderDto> AssignDeliveryAddress(Address address);
    public Task<OrderDto> AddItem(OrderItem item);
    public Task<OrderDto> RemoveItem(Guid itemId);
    public Task<OrderDto> ConfirmOrder();
    public Task<OrderDto> ConfirmPayment();
    public Task<OrderDto> StartProcessing();
    public Task<OrderDto> FinishedItem(Guid itemId);
    public Task<OrderDto> Served();
    public Task<OrderDto> StartDelivery();
    public Task<OrderDto> Delivered();
}