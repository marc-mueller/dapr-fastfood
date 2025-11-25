using FinanceService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceService.Storage.EntityConfigurations;

public class CustomerConfiguration : RelationalEntityConfigurationBase<Customer>
{
    protected override string TableName { get; } = "Customer";
    protected override void ConfigureEntity(EntityTypeBuilder<Customer> entity)
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired(false);
        entity.Property(e => e.LastName).HasMaxLength(100).IsRequired(false);

        // Store addresses as JSON columns
        entity.OwnsOne(e => e.InvoiceAddress, nav =>
        {
            nav.ToJson();
            nav.Property(a => a.Street).HasMaxLength(200);
            nav.Property(a => a.StreetNumber).HasMaxLength(20);
            nav.Property(a => a.ZipCode).HasMaxLength(20);
            nav.Property(a => a.City).HasMaxLength(100);
            nav.Property(a => a.Country).HasMaxLength(100);
        });

        entity.OwnsOne(e => e.DeliveryAddress, nav =>
        {
            nav.ToJson();
            nav.Property(a => a.Street).HasMaxLength(200);
            nav.Property(a => a.StreetNumber).HasMaxLength(20);
            nav.Property(a => a.ZipCode).HasMaxLength(20);
            nav.Property(a => a.City).HasMaxLength(100);
            nav.Property(a => a.Country).HasMaxLength(100);
        });
    }
}
