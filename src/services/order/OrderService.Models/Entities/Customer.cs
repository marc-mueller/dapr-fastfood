namespace OrderService.Models.Entities;

public class Customer
{
    public Guid Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public Address? InvoiceAddress { get; set; }
    public Address? DeliveryAddress { get; set; }
}