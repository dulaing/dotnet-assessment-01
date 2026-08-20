namespace Library.Application.Contracts.Members
{
    public record CreateMemberRequest (
        string FullName, 
        string Email, 
        string? PhoneNumber
    );

    public record MemberResponse(
        int Id, 
        string FullName, 
        string Email, 
        string? PhoneNumber, 
        DateTime RegisteredDate, 
        bool IsActive
    );
    
    public record UpdateMemberRequest(
        string FullName, 
        string Email, 
        string? PhoneNumber, 
        bool IsActive
    );
}