using FinanceService.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceService.Storage.EntityConfigurations;

public class OrderConfiguration : RelationalEntityConfigurationBase<Order>
{
    protected override string TableName { get; } = "Order";
    
    protected override void ConfigureEntity(EntityTypeBuilder<Order> entity)
    {
        entity.HasKey(e => e.Id);
        
        entity.Property(e => e.Type)
            .HasConversion<int>()
            .IsRequired();
        
        entity.Property(e => e.State)
            .HasConversion<int>()
            .IsRequired();
        
        entity.Property(e => e.ServiceFee)
            .HasPrecision(18, 2)
            .IsRequired(false);
        
        entity.Property(e => e.Discount)
            .HasPrecision(18, 2)
            .IsRequired(false);
        
        entity.Property(e => e.CustomerComments)
            .HasMaxLength(1000)
            .IsRequired(false);
        
        entity.Property(e => e.CreatedAt)
            .IsRequired();
        
        entity.Property(e => e.ClosedAt)
            .IsRequired(false);
        
        // Foreign key relationship to Customer
        entity.HasOne(e => e.Customer)
            .WithMany()
            .HasForeignKey(e => e.CustomerId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        
        // One-to-many relationship with OrderItems
        entity.HasMany(e => e.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Add indexes for performance
        entity.HasIndex(e => e.CreatedAt);
        entity.HasIndex(e => e.State);
        entity.HasIndex(e => e.CustomerId);
    }
}
