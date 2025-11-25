using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceService.Storage.EntityConfigurations;

public abstract class RelationalEntityConfigurationBase<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class
{
    protected RelationalEntityConfigurationBase()
    {
    }
    
    protected abstract string TableName { get; }

    public void Configure(EntityTypeBuilder<TEntity> builder)
    {
        if (builder is null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        ConfigureEntity(builder);
        ConfigureTableMapping(builder);
    }

    protected abstract void ConfigureEntity(EntityTypeBuilder<TEntity> entity);

    protected virtual void ConfigureTableMapping(EntityTypeBuilder<TEntity> entityTypeBuilder)
    {
        entityTypeBuilder.ToTable(TableName);
    }
}