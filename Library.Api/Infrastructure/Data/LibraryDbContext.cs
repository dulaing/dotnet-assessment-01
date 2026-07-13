using Library.Api.Application.Abstractions;
using Library.Api.Domain.Common;
using Library.Api.Domain.Entities;
using Library.Api.Infrastructure.Data.Converters;
using Microsoft.EntityFrameworkCore;

namespace Library.Api.Infrastructure.Data;

// Central EF Core unit of work for the library schema.
public sealed class LibraryDbContext(DbContextOptions<LibraryDbContext> options, ICurrentUserService currentUser)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Borrowing> Borrowings => Set<Borrowing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Each table's mapping lives in its own IEntityTypeConfiguration in Data/Configurations.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibraryDbContext).Assembly);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        // One UTC rule for every DateTime instead of per-entity normalization.
        configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    public override int SaveChanges()
    {
        ApplyAuditInfo();
        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditInfo();
        return base.SaveChangesAsync(cancellationToken);
    }

    // Stamps created/updated fields right before saving so callers never set them by hand.
    private void ApplyAuditInfo()
    {
        var timestamp = DateTime.UtcNow;
        var user = currentUser.UserId;

        foreach (var entry in ChangeTracker.Entries<IAuditable>())
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = timestamp;
                entry.Entity.CreatedBy = user;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = timestamp;
                entry.Entity.UpdatedBy = user;
            }
        }
    }
}
