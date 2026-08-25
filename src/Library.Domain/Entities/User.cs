using Library.Domain.Enums;

namespace Library.Domain.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; }

        // null for admins, since a librarian is not a borrower
        public int? MemberId { get; set; }
    }
}