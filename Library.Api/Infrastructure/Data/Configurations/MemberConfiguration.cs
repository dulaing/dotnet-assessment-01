using Library.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Library.Api.Infrastructure.Data.Configurations;

// Owns everything about the Members table: shape, constraints, and seed rows.
public sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(member => member.Id);
        builder.Property(member => member.FullName).HasMaxLength(150);
        builder.Property(member => member.Email).HasMaxLength(256);
        builder.Property(member => member.PhoneNumber).HasMaxLength(20);
        builder.HasIndex(member => member.Email).IsUnique();
        builder.ConfigureAudit();

        builder.HasData(
            new Member
            {
                Id = 2001,
                FullName = "Ada Lovelace",
                Email = "ada.seed@example.com",
                PhoneNumber = "555-1001",
                RegisteredDate = SeedDefaults.SeedTimestamp,
                IsActive = true,
                CreatedAtUtc = SeedDefaults.SeedTimestamp,
                CreatedBy = SeedDefaults.SeedUser
            },
            new Member
            {
                Id = 2002,
                FullName = "Grace Hopper",
                Email = "grace.seed@example.com",
                PhoneNumber = "555-1002",
                RegisteredDate = SeedDefaults.SeedTimestamp,
                IsActive = true,
                CreatedAtUtc = SeedDefaults.SeedTimestamp,
                CreatedBy = SeedDefaults.SeedUser
            });
    }
}
