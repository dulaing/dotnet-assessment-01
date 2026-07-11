using Library.Api.Contracts.Borrowings;

namespace Library.Api.Application.Interfaces;

// Defines the application-level use cases for borrowing workflows.
public interface IBorrowingService
{
    Task<BorrowingResponse> CreateAsync(CreateBorrowingRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<BorrowingResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BorrowingResponse>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken);
    Task<BorrowingResponse> ReturnAsync(int id, CancellationToken cancellationToken);
}
