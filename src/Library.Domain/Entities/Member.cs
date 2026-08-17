using Library.Domain.Common;

namespace Library.Domain.Entities
{
    public class Member
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? PhoneNumber {  get; set; }
        public DateTime RegisteredDate { get; set; }
        public bool IsActive { get; set; }

        public Result<Member> EnsureCanBorrow()
        {
            if (!IsActive) {
                return Result<Member>.Validation("member_inactive", "Inactive members cannot borrow books.");
            }

            return Result<Member>.Success(this);
        }

    }
}
