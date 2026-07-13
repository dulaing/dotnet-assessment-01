using Library.Api.Domain.Common;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Api.Infrastructure.Data.Configurations;

// Shared column setup for audit fields so each table configuration stays DRY.
public static class AuditableConfigurationExtensions
{
    public static void ConfigureAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditable
    {
        builder.Property(entity => entity.CreatedBy).HasMaxLength(100);
        builder.Property(entity => entity.UpdatedBy).HasMaxLength(100);
    }
}
