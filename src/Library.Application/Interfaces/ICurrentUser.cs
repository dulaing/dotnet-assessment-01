namespace Library.Application.Interfaces
{
    // lets services ask who is calling without knowing anything about http or tokens
    public interface ICurrentUser
    {
        bool IsAuthenticated { get; }
        bool IsAdmin { get; }
        int? UserId { get; }

        // null for admins, since a librarian has no member record
        int? MemberId { get; }
    }
}
