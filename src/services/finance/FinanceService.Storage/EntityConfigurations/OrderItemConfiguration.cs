using FinanceService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceService.Storage.EntityConfigurations;

public class OrderItemConfiguration : RelationalEntityConfigurationBase<OrderItem>
{
    protected override string TableName { get; } = "OrderItem";
    
    protected override void ConfigureEntity(EntityTypeBuilder<OrderItem> entity)
    {
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.ProductId)
            .IsRequired();
        
        entity.Property(e => e.ProductDescription)
            .HasMaxLength(500)
            .IsRequired(false);
        
        entity.Property(e => e.CustomerComments)
            .HasMaxLength(500)
            .IsRequired(false);
        
        entity.Property(e => e.Quantity)
            .IsRequired();
        
        entity.Property(e => e.ItemPrice)
            .HasPrecision(18, 2)
            .IsRequired();
        
        entity.Property(e => e.State)
            .HasConversion<int>()
            .IsRequired();
        
        // Foreign key is configured in OrderConfiguration (HasMany/WithOne)
        entity.Property(e => e.OrderId)
            .IsRequired();
        
        // Add indexes
        entity.HasIndex(e => e.OrderId);
        entity.HasIndex(e => e.State);
    }
}
