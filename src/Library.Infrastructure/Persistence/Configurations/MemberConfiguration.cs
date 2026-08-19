using Library.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Infrastructure.Persistence.Configurations
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.ToTable("members");
            builder.HasKey(m => m.Id);

            builder.Property(m => m.FullName).IsRequired().HasMaxLength(200);
            builder.Property(m => m.Email).IsRequired().HasMaxLength(320);
            builder.Property(m => m.PhoneNumber).HasMaxLength(30);

            builder.HasIndex(m => m.Email).IsUnique();
        }
    }
}