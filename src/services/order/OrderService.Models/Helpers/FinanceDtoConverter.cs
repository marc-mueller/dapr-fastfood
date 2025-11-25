using FinanceService.Common.Dtos;
using OrderService.Models.Entities;

namespace OrderService.Models.Helpers;

public static class FinanceDtoConverter
{
    public static OrderDto ToFinanceDto(this Order order)
    {
        return new OrderDto(){ Id = order.Id, State = (OrderDtoState)order.State, Type = (OrderDtoType)order.Type, Items = order.Items?.Select(i => i.ToFinanceDto()).ToList(), Customer = order.Customer?.ToFinanceDto()};
    }
    
    /// <summary>
    /// Converts Order to Finance DTO with pricing breakdown (service fees and discounts).
    /// </summary>
    public static OrderDto ToFinanceDto(this Order order, decimal? serviceFee, decimal? discount)
    {
        return new OrderDto()
        { 
            Id = order.Id, 
            State = (OrderDtoState)order.State, 
            Type = (OrderDtoType)order.Type, 
            Items = order.Items?.Select(i => i.ToFinanceDto()).ToList(), 
            Customer = order.Customer?.ToFinanceDto(),
            ServiceFee = serviceFee,
            Discount = discount
        };
    }

    public static OrderItemDto ToFinanceDto(this OrderItem item)
    {
        return new OrderItemDto() { Id = item.Id, ItemPrice = item.ItemPrice, ProductDescription = item.ProductDescription, ProductId = item.ProductId, Quantity = item.Quantity, State = (OrderItemDtoState)item.State };
    }
    
    public static CustomerDto ToFinanceDto(this Customer customer)
    {
        return new CustomerDto() { Id = customer.Id, FirstName = customer.FirstName, LastName = customer.LastName, InvoiceAddress = customer.InvoiceAddress?.ToFinanceDto(), DeliveryAddress = customer.DeliveryAddress?.ToFinanceDto() };
    }
    
    public static AddressDto ToFinanceDto(this Address address)
    {
        return new AddressDto() { Street = address.Street, City = address.City, ZipCode = address.ZipCode, Country = address.Country };
    }
}