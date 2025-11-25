using Microsoft.EntityFrameworkCore;

namespace FinanceService.Storage.EntityConfigurations;

public interface IRelationalEntityConfiguration
{
    void Configure(ModelBuilder modelBuilder);
}