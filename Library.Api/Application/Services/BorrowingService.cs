using Library.Api.Application.Exceptions;
using Library.Api.Application.Interfaces;
using Library.Api.Contracts.Borrowings;
using Library.Api.Domain.Entities;
using Library.Api.Domain.Enums;
using Library.Api.Infrastructure.Repositories;

namespace Library.Api.Application.Services;

// Holds borrowing-specific business rules outside the HTTP layer.
public sealed class BorrowingService(
    IBorrowingRepository borrowingRepository,
    IBookRepository bookRepository,
    IMemberRepository memberRepository) : IBorrowingService
{
    public async Task<BorrowingResponse> CreateAsync(CreateBorrowingRequest request, CancellationToken cancellationToken)
    {
        var book = await bookRepository.GetByIdAsync(request.BookId, cancellationToken)
            ?? throw new NotFoundException("Book not found");
        var member = await memberRepository.GetByIdAsync(request.MemberId, cancellationToken)
            ?? throw new NotFoundException("Member not found");

        if (!member.IsActive)
        {
            throw new ConflictException("Member is inactive");
        }

        var activeBorrowings = await borrowingRepository.CountActiveByMemberIdAsync(request.MemberId, cancellationToken);

        if (activeBorrowings >= 3)
        {
            throw new ConflictException("Member borrowing limit exceeded");
        }

        try
        {
            book.BorrowCopy();
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        var borrowedDate = DateTime.UtcNow;
        var borrowing = new Borrowing
        {
            BookId = request.BookId,
            MemberId = request.MemberId,
            BorrowedDate = borrowedDate,
            DueDate = borrowedDate.AddDays(14),
            Status = BorrowingStatus.Borrowed
        };

        await borrowingRepository.AddAsync(borrowing, cancellationToken);
        bookRepository.Update(book);
        await borrowingRepository.SaveChangesAsync(cancellationToken);

        return MapResponse(borrowing);
    }

    public async Task<IReadOnlyList<BorrowingResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var borrowings = await borrowingRepository.GetAllAsync(cancellationToken);

        return borrowings.Select(MapResponse).ToArray();
    }

    public async Task<IReadOnlyList<BorrowingResponse>> GetByMemberIdAsync(int memberId, CancellationToken cancellationToken)
    {
        _ = await memberRepository.GetByIdAsync(memberId, cancellationToken)
            ?? throw new NotFoundException("Member not found");

        var borrowings = await borrowingRepository.GetByMemberIdAsync(memberId, cancellationToken);

        return borrowings.Select(MapResponse).ToArray();
    }

    public async Task<BorrowingResponse> ReturnAsync(int id, CancellationToken cancellationToken)
    {
        var borrowing = await borrowingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Borrowing record not found");
        var book = await bookRepository.GetByIdAsync(borrowing.BookId, cancellationToken)
            ?? throw new NotFoundException("Book not found");

        try
        {
            borrowing.ReturnBook(DateTime.UtcNow);
            book.ReturnCopy();
        }
        catch (InvalidOperationException exception)
        {
            throw new ConflictException(exception.Message);
        }

        bookRepository.Update(book);
        await borrowingRepository.SaveChangesAsync(cancellationToken);

        return MapResponse(borrowing);
    }

    private static BorrowingResponse MapResponse(Borrowing borrowing)
    {
        return new BorrowingResponse
        {
            Id = borrowing.Id,
            BookId = borrowing.BookId,
            MemberId = borrowing.MemberId,
            BorrowedDate = borrowing.BorrowedDate,
            DueDate = borrowing.DueDate,
            ReturnedDate = borrowing.ReturnedDate,
            Status = borrowing.Status.ToString()
        };
    }
}
