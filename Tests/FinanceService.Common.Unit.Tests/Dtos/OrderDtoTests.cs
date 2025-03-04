using FinanceService.Common.Dtos;
using Xunit;

namespace FinanceService.Common.Unit.Tests.Dtos;

public class OrderDtoTests
{
    [Fact]
    public void OrderDto_Constructor_InitializesItemsCollection()
    {
        // Act
        var order = new OrderDto();

        // Assert
        Assert.NotNull(order.Items);
        Assert.Empty(order.Items);
    }

    [Fact]
    public void OrderDto_PropertiesAreAssignable()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var customer = new CustomerDto { FirstName = "Test", LastName = "Customer"};
        var orderItem = new OrderItemDto { ProductDescription = "Test Item" };

        // Act
        var order = new OrderDto
        {
            Id = orderId,
            Type = OrderDtoType.Inhouse,
            State = OrderDtoState.Confirmed,
            Customer = customer,
            CustomerComments = "Test Comment"
        };
        order.Items.Add(orderItem);

        // Assert
        Assert.Equal(orderId, order.Id);
        Assert.Equal(OrderDtoType.Inhouse, order.Type);
        Assert.Equal(OrderDtoState.Confirmed, order.State);
        Assert.Equal(customer, order.Customer);
        Assert.Equal("Test Comment", order.CustomerComments);
        Assert.Single(order.Items);
        Assert.Equal(orderItem, order.Items.First());
    }
}