using System.Collections.ObjectModel;
using KitchenService.Common.Dtos;
using KitchenService.Entities;

namespace KitchenService.Helpers;

public static class DtoConverter
{
    public static KitchenOrderDto ToDto(this KitchenOrder order)
    {
        return new KitchenOrderDto(){ Id = order.Id,  Items = order.Items?.Select(i => i.ToDto()).ToList() ?? new List<KitchenOrderItemDto>()};
    }

    public static KitchenOrderItemDto ToDto(this KitchenOrderItem item)
    {
        return new KitchenOrderItemDto(){ Id = item.Id, ProductId = item.ProductId, Quantity = item.Quantity, CustomerComments = item.CustomerComments, OrderId = item.OrderId, State = (KitchenOrderItemDtoState)item.State , ProductDescription = item.ProductDescription};
    }
    
    public static KitchenOrder ToEntity(this KitchenOrderDto order)
    {
        return new KitchenOrder(){ Id = order.Id, Items = order.Items?.Select(i => i.ToEntity()).ToList() ?? new List<KitchenOrderItem>()};
    }
    
    public static KitchenOrderItem ToEntity(this KitchenOrderItemDto item)
    {
        return new KitchenOrderItem(){ Id = item.Id, ProductId = item.ProductId, Quantity = item.Quantity, CustomerComments = item.CustomerComments, OrderId = item.OrderId, State = (KitchenOrderItemState)item.State, ProductDescription = item.ProductDescription};
    }
}