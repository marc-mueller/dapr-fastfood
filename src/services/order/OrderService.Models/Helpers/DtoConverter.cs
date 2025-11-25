using OrderService.Common.Dtos;
using OrderService.Models.Entities;

namespace OrderService.Models.Helpers;

public static class DtoConverter
{
    public static OrderDto ToDto(this Order order)
    {
        return new OrderDto(){ Id = order.Id, OrderReference = order.OrderReference, State = (OrderDtoState)order.State, Type = (OrderDtoType)order.Type, Items = order.Items?.Select(i => i.ToDto()).ToList(), Customer = order.Customer?.ToDto()};
    }

    public static OrderItemDto ToDto(this OrderItem item)
    {
        return new OrderItemDto() { Id = item.Id, ItemPrice = item.ItemPrice, ProductDescription = item.ProductDescription, ProductId = item.ProductId, Quantity = item.Quantity, State = (OrderItemDtoState)item.State };
    }
    
    public static CustomerDto ToDto(this Customer customer)
    {
        return new CustomerDto() { Id = customer.Id, FirstName = customer.FirstName, LastName = customer.LastName, LoyaltyNumber = customer.LoyaltyNumber, InvoiceAddress = customer.InvoiceAddress?.ToDto(), DeliveryAddress = customer.DeliveryAddress?.ToDto() };
    }
    
    public static AddressDto ToDto(this Address address)
    {
        return new AddressDto() { Street = address.Street, City = address.City, ZipCode = address.ZipCode, Country = address.Country };
    }
    
    public static Order ToEntity(this OrderDto order)
    {
        return new Order(){ Id = order.Id, OrderReference = order.OrderReference, State = (OrderState)order.State, Type = (OrderType)order.Type, Items = order.Items?.Select(i => i.ToEntity()).ToList(), Customer = order.Customer?.ToEntity()};
    }
    
    public static OrderItem ToEntity(this OrderItemDto item)
    {
        return new OrderItem() { Id = item.Id, ItemPrice = item.ItemPrice, ProductDescription = item.ProductDescription, ProductId = item.ProductId, Quantity = item.Quantity, State = (OrderItemState)item.State };
    }
    
    public static Customer ToEntity(this CustomerDto customer)
    {
        return new Customer() { Id = customer.Id, FirstName = customer.FirstName, LastName = customer.LastName, LoyaltyNumber = customer.LoyaltyNumber, InvoiceAddress = customer.InvoiceAddress?.ToEntity(), DeliveryAddress = customer.DeliveryAddress?.ToEntity()};
    }
    
    public static Address ToEntity(this AddressDto address)
    {
        return new Address() { Street = address.Street, City = address.City, ZipCode = address.ZipCode, Country = address.Country };
    }
    
   
}