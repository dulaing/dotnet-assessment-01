using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Library.Api.Infrastructure.Data.Converters;

// Postgres timestamptz requires UTC. This keeps every stored DateTime in UTC and
// tags values read back from the database as UTC, so entities no longer normalize dates themselves.
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => value.Kind == DateTimeKind.Utc
                ? value
                : value.Kind == DateTimeKind.Unspecified
                    ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                    : value.ToUniversalTime(),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}
