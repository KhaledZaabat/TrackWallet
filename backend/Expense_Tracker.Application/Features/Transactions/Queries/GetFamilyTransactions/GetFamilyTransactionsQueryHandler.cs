using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;

public sealed class GetFamilyTransactionsQueryHandler(
    IAppDbContext db,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder
) : IRequestHandler<GetFamilyTransactionsQuery, Result<CursorPagedResponse<TransactionItem>>>
{
    public async Task<Result<CursorPagedResponse<TransactionItem>>> Handle(
        GetFamilyTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        // Parse cursor (using CreatedAtUtc as cursor)
        DateTimeOffset cursor;
        if (string.IsNullOrWhiteSpace(request.Cursor))
        {
            cursor = DateTimeOffset.UtcNow;
        }
        else if (!DateTimeOffset.TryParse(request.Cursor, out cursor))
        {
            cursor = DateTimeOffset.UtcNow;
        }

        // Validate and set page size (max 50)
        int size = request.PageSize is <= 0 or > 50 ? 20 : request.PageSize;

        // Build query with cursor pagination
        var query = db.Transactions
            .AsNoTracking()
            .Where(t =>
                t.FamilyId == request.FamilyId &&
                t.CreatedAtUtc < cursor)
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .Take(size)
            .Select(t => new TransactionItem(
                TransactionId: t.Id,
                Title: t.Title,
                Amount: t.Amount,
                Type: t.Type,
                TransactedOn: t.TransactedOn,
                CreatedAtUtc: t.CreatedAtUtc,
                Category: new CategoryResponse(
                    CategoryId: t.Category!.Id,
                    Name: t.Category.Name,
                    Icon: t.Category.IconName
                ),
                Creator: new CreatorResponse(
                    UserId: t.CreatedBy!.Id,
                    FullName: t.CreatedBy.FullName,
                    ProfileImageUrl: t.CreatedBy.ProfileImageFileId.HasValue
                        ? fileUrlBuilder.GetUrl(t.CreatedBy.ProfileImageFileId.Value)
                        : null
                )
            ));

        // Execute query
        var transactions = await query.ToListAsync(cancellationToken);

        // Handle empty results
        if (transactions.Count == 0)
        {
            return Result.Success(
                new CursorPagedResponse<TransactionItem>(
                    Items: Array.Empty<TransactionItem>(),
                    NextCursor: null,
                    HasNextPage: false));
        }

        // Calculate next cursor
        string? nextCursor = transactions.Count == size
            ? transactions.Last().CreatedAtUtc.ToString("o")
            : null;

        // Build response
        return Result.Success(
            new CursorPagedResponse<TransactionItem>(
                Items: transactions,
                NextCursor: nextCursor,
                HasNextPage: nextCursor is not null));
    }
}