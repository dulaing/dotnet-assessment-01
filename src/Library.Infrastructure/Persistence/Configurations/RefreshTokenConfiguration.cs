using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations
{
    // Defines refresh-token storage constraints and its owning account relationship.
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens");
            builder.HasKey(token => token.Id);

            builder.Property(token => token.TokenHash).IsRequired().HasMaxLength(64);
            builder.Property(token => token.ReplacedByTokenHash).HasMaxLength(64);
            builder.HasIndex(token => token.TokenHash).IsUnique();
            builder.HasIndex(token => token.UserId);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
