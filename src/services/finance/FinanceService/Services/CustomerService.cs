using FinanceService.Common.Dtos;
using FinanceService.Entities;
using FinanceService.Storage.UnitOfWork;

namespace FinanceService.Services;

public interface ICustomerService
{
    Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto, CancellationToken cancellationToken = default);
    Task<CustomerDto?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default);
    Task UpdateCustomerAsync(CustomerDto customerDto, CancellationToken cancellationToken = default);
}

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IUnitOfWork unitOfWork, ILogger<CustomerService> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CustomerDto> CreateCustomerAsync(CustomerDto customerDto, CancellationToken cancellationToken = default)
    {
        if (customerDto == null) throw new ArgumentNullException(nameof(customerDto));

        var customer = MapToEntity(customerDto);
        
        await _unitOfWork.Customers.AddAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerId} created in database", customer.Id);

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> GetCustomerAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(customerId, cancellationToken);
        return customer != null ? MapToDto(customer) : null;
    }

    public async Task UpdateCustomerAsync(CustomerDto customerDto, CancellationToken cancellationToken = default)
    {
        if (customerDto == null) throw new ArgumentNullException(nameof(customerDto));

        var customer = MapToEntity(customerDto);
        
        await _unitOfWork.Customers.UpdateAsync(customer, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Customer {CustomerId} updated in database", customer.Id);
    }

    private Customer MapToEntity(CustomerDto dto)
    {
        return new Customer
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            InvoiceAddress = dto.InvoiceAddress != null ? new Address
            {
                Street = dto.InvoiceAddress.Street,
                StreetNumber = dto.InvoiceAddress.StreetNumber,
                ZipCode = dto.InvoiceAddress.ZipCode,
                City = dto.InvoiceAddress.City,
                Country = dto.InvoiceAddress.Country
            } : null,
            DeliveryAddress = dto.DeliveryAddress != null ? new Address
            {
                Street = dto.DeliveryAddress.Street,
                StreetNumber = dto.DeliveryAddress.StreetNumber,
                ZipCode = dto.DeliveryAddress.ZipCode,
                City = dto.DeliveryAddress.City,
                Country = dto.DeliveryAddress.Country
            } : null
        };
    }

    private CustomerDto MapToDto(Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            InvoiceAddress = entity.InvoiceAddress != null ? new AddressDto
            {
                Street = entity.InvoiceAddress.Street,
                StreetNumber = entity.InvoiceAddress.StreetNumber,
                ZipCode = entity.InvoiceAddress.ZipCode,
                City = entity.InvoiceAddress.City,
                Country = entity.InvoiceAddress.Country
            } : null,
            DeliveryAddress = entity.DeliveryAddress != null ? new AddressDto
            {
                Street = entity.DeliveryAddress.Street,
                StreetNumber = entity.DeliveryAddress.StreetNumber,
                ZipCode = entity.DeliveryAddress.ZipCode,
                City = entity.DeliveryAddress.City,
                Country = entity.DeliveryAddress.Country
            } : null
        };
    }
}
