namespace Library.Api.Application.Abstractions;

// The seam for identity. Returns who is acting so audit fields can be stamped.
// Real user identity plugs in here once authentication exists.
public interface ICurrentUserService
{
    string? UserId { get; }
}
