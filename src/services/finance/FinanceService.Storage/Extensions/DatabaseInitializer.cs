using FinanceService.Entities;
using FinanceService.Storage.Storages;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Storage.Extensions;

public static class DatabaseInitializer
{
    /// <summary>
    /// Seeds the database with sample data for demo purposes
    /// </summary>
    public static async Task SeedDemoDataAsync(this FinanceStorage context)
    {
        if (await context.Customers.AnyAsync() || await context.Orders.AnyAsync())
        {
            // Database already has data
            return;
        }

        var demoCustomers = new List<Customer>
        {
            new Customer
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "John",
                LastName = "Doe",
                InvoiceAddress = new Address
                {
                    Street = "Main Street",
                    StreetNumber = "123",
                    ZipCode = "12345",
                    City = "New York",
                    Country = "USA"
                },
                DeliveryAddress = new Address
                {
                    Street = "Main Street",
                    StreetNumber = "123",
                    ZipCode = "12345",
                    City = "New York",
                    Country = "USA"
                }
            },
            new Customer
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Jane",
                LastName = "Smith",
                InvoiceAddress = new Address
                {
                    Street = "Oak Avenue",
                    StreetNumber = "456",
                    ZipCode = "54321",
                    City = "Los Angeles",
                    Country = "USA"
                }
            }
        };

        var demoOrders = new List<Order>
        {
            new Order
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Type = OrderType.Delivery,
                State = OrderState.Closed,
                CustomerId = demoCustomers[0].Id,
                Customer = demoCustomers[0],
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        ProductId = Guid.NewGuid(),
                        ProductDescription = "Cheeseburger",
                        Quantity = 2,
                        ItemPrice = 8.99m,
                        State = OrderItemState.Delivered
                    },
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                        ProductId = Guid.NewGuid(),
                        ProductDescription = "French Fries",
                        Quantity = 1,
                        ItemPrice = 3.99m,
                        State = OrderItemState.Delivered
                    }
                },
                ServiceFee = null,
                Discount = 2.00m,
                CustomerComments = "Extra ketchup please",
                CreatedAt = DateTime.UtcNow.AddHours(-2),
                ClosedAt = DateTime.UtcNow.AddHours(-1)
            },
            new Order
            {
                Id = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Type = OrderType.TakeAway,
                State = OrderState.Paid,
                CustomerId = demoCustomers[1].Id,
                Customer = demoCustomers[1],
                Items = new List<OrderItem>
                {
                    new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                        ProductId = Guid.NewGuid(),
                        ProductDescription = "Chicken Sandwich",
                        Quantity = 1,
                        ItemPrice = 7.99m,
                        State = OrderItemState.InPreparation
                    }
                },
                ServiceFee = 1.50m,
                Discount = null,
                CreatedAt = DateTime.UtcNow.AddMinutes(-15)
            }
        };

        await context.Customers.AddRangeAsync(demoCustomers);
        await context.Orders.AddRangeAsync(demoOrders);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Clears all data from the database
    /// </summary>
    public static async Task ClearAllDataAsync(this FinanceStorage context)
    {
        context.Orders.RemoveRange(context.Orders);
        context.Customers.RemoveRange(context.Customers);
        await context.SaveChangesAsync();
    }
}
