namespace Library.Api.Infrastructure.Data.Configurations;

// Fixed values for seeded rows. HasData needs constant values, so audit fields
// on seed data cannot use DateTime.UtcNow and are stamped here instead.
internal static class SeedDefaults
{
    public static readonly DateTime SeedTimestamp = new(2026, 7, 10, 0, 0, 0, DateTimeKind.Utc);
    public const string SeedUser = "seed";
}
