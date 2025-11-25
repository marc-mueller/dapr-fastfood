using FinanceService.Entities;
using FinanceService.Storage.Storages;
using Microsoft.EntityFrameworkCore;

namespace FinanceService.Storage.Repositories;

public interface ICustomerRepository : IRepository<Customer>
{
}

public class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    public CustomerRepository(FinanceStorage context) : base(context)
    {
    }
}