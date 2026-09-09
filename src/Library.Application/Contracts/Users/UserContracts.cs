namespace Library.Application.Contracts.Users
{
    // Defines the credentials and identity link an admin assigns to a new account.
    public record CreateUserRequest(string Email, string Password, string Role, int? MemberId);

    // Returns account identity without ever exposing password data.
    public record UserResponse(int Id, string Email, string Role, int? MemberId);
}
