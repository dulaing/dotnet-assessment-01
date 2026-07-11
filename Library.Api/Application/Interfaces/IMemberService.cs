using Library.Api.Contracts.Members;

namespace Library.Api.Application.Interfaces;

// Defines the application-level use cases for member operations.
public interface IMemberService
{
    Task<IReadOnlyList<MemberResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<MemberResponse> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<MemberResponse> CreateAsync(CreateMemberRequest request, CancellationToken cancellationToken);
    Task<MemberResponse> UpdateAsync(int id, UpdateMemberRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
