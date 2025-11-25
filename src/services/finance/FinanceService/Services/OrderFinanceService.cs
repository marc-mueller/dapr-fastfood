using FinanceService.Common.Dtos;
using FinanceService.Entities;
using FinanceService.Storage.UnitOfWork;

namespace FinanceService.Services;

public interface IOrderFinanceService
{
    Task<OrderDto> CreateOrderAsync(OrderDto orderDto, CancellationToken cancellationToken = default);
    Task CloseOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderDto?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default);
}

public class OrderFinanceService : IOrderFinanceService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<OrderFinanceService> _logger;

    public OrderFinanceService(IUnitOfWork unitOfWork, ILogger<OrderFinanceService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<OrderDto> CreateOrderAsync(OrderDto orderDto, CancellationToken cancellationToken = default)
    {
        if (orderDto == null) throw new ArgumentNullException(nameof(orderDto));

        var order = MapToEntity(orderDto);
        order.CreatedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.AddAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} created in database", order.Id);

        return MapToDto(order);
    }

    public async Task CloseOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        
        if (order == null)
        {
            _logger.LogWarning("Order {OrderId} not found for closing", orderId);
            throw new InvalidOperationException($"Order {orderId} not found");
        }

        order.State = OrderState.Closed;
        order.ClosedAt = DateTime.UtcNow;

        await _unitOfWork.Orders.UpdateAsync(order, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order {OrderId} closed in database", orderId);
    }

    public async Task<OrderDto?> GetOrderAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        var order = await _unitOfWork.Orders.GetByIdAsync(orderId, cancellationToken);
        return order != null ? MapToDto(order) : null;
    }

    private Order MapToEntity(OrderDto dto)
    {
        return new Order
        {
            Id = dto.Id,
            Type = (OrderType)(int)dto.Type,
            State = (OrderState)(int)dto.State,
            CustomerId = dto.Customer?.Id,
            Customer = dto.Customer != null ? new Customer
            {
                Id = dto.Customer.Id,
                FirstName = dto.Customer.FirstName,
                LastName = dto.Customer.LastName,
                InvoiceAddress = dto.Customer.InvoiceAddress != null ? new Address
                {
                    Street = dto.Customer.InvoiceAddress.Street,
                    StreetNumber = dto.Customer.InvoiceAddress.StreetNumber,
                    ZipCode = dto.Customer.InvoiceAddress.ZipCode,
                    City = dto.Customer.InvoiceAddress.City,
                    Country = dto.Customer.InvoiceAddress.Country
                } : null,
                DeliveryAddress = dto.Customer.DeliveryAddress != null ? new Address
                {
                    Street = dto.Customer.DeliveryAddress.Street,
                    StreetNumber = dto.Customer.DeliveryAddress.StreetNumber,
                    ZipCode = dto.Customer.DeliveryAddress.ZipCode,
                    City = dto.Customer.DeliveryAddress.City,
                    Country = dto.Customer.DeliveryAddress.Country
                } : null
            } : null,
            Items = dto.Items?.Select(i => new OrderItem
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                ItemPrice = i.ItemPrice,
                ProductDescription = i.ProductDescription,
                CustomerComments = i.CustomerComments,
                State = (OrderItemState)(int)i.State,
                OrderId = dto.Id  // Set the foreign key
            }).ToList(),
            ServiceFee = dto.ServiceFee,
            Discount = dto.Discount,
            CustomerComments = dto.CustomerComments
        };
    }

    private OrderDto MapToDto(Order entity)
    {
        return new OrderDto
        {
            Id = entity.Id,
            Type = (OrderDtoType)(int)entity.Type,
            State = (OrderDtoState)(int)entity.State,
            Customer = entity.Customer != null ? new CustomerDto
            {
                Id = entity.Customer.Id,
                FirstName = entity.Customer.FirstName,
                LastName = entity.Customer.LastName,
                InvoiceAddress = entity.Customer.InvoiceAddress != null ? new AddressDto
                {
                    Street = entity.Customer.InvoiceAddress.Street,
                    StreetNumber = entity.Customer.InvoiceAddress.StreetNumber,
                    ZipCode = entity.Customer.InvoiceAddress.ZipCode,
                    City = entity.Customer.InvoiceAddress.City,
                    Country = entity.Customer.InvoiceAddress.Country
                } : null,
                DeliveryAddress = entity.Customer.DeliveryAddress != null ? new AddressDto
                {
                    Street = entity.Customer.DeliveryAddress.Street,
                    StreetNumber = entity.Customer.DeliveryAddress.StreetNumber,
                    ZipCode = entity.Customer.DeliveryAddress.ZipCode,
                    City = entity.Customer.DeliveryAddress.City,
                    Country = entity.Customer.DeliveryAddress.Country
                } : null
            } : null,
            Items = entity.Items?.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                ItemPrice = i.ItemPrice,
                ProductDescription = i.ProductDescription,
                CustomerComments = i.CustomerComments,
                State = (OrderItemDtoState)(int)i.State
            }).ToList(),
            ServiceFee = entity.ServiceFee,
            Discount = entity.Discount,
            CustomerComments = entity.CustomerComments
        };
    }
}
