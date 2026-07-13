using Library.Api.Application.Abstractions;

namespace Library.Api.Infrastructure.CurrentUser;

// No authentication yet, so all writes are attributed to the system.
public sealed class SystemCurrentUserService : ICurrentUserService
{
    public string? UserId => "system";
}
