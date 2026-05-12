using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Contracts.Reponses.Transaction;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;

public sealed class GetFamilyTransactionsQueryHandler(
    IRepository<Transaction> transactionRepo,
    [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder
)
{
    public async Task<ErrorOr<CursorPagedResponse<TransactionItem>>> Handle(
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

        // Build query with cursor pagination and filters
        var query = transactionRepo.Query()
            .Where(t =>
                t.FamilyId == request.FamilyId &&
                t.CreatedAtUtc < cursor);

        // Filter by transaction type
        if (request.TransactionType.HasValue)
        {
            query = query.Where(t => t.Type == request.TransactionType.Value);
        }

        // Filter by category type
        if (request.CategoryType.HasValue)
        {
            query = query.Where(t => t.Category!.Type == request.CategoryType.Value);
        }

        // Filter by minimum amount
        if (request.MinAmount.HasValue)
        {
            query = query.Where(t => t.Amount >= request.MinAmount.Value);
        }

        // Filter by maximum amount
        if (request.MaxAmount.HasValue)
        {
            query = query.Where(t => t.Amount <= request.MaxAmount.Value);
        }

        // Filter by creator (user who created the transaction)
        if (request.CreatorId.HasValue)
        {
            query = query.Where(t => t.CreatedById == request.CreatorId.Value);
        }

        // Apply ordering and pagination
        query = query
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .Take(size);

        // Select transaction items
        var transactions = await query
            .Select(t => new TransactionItem(
                TransactionId: t.Id,
                Title: t.Title,
                Amount: t.Amount,
                Type: t.Type,
                TransactedOn: t.TransactedOn,
                CreatedAtUtc: t.CreatedAtUtc,
                Category: new CategoryResponse(
                    CategoryId: t.Category!.Id,
                    Name: t.Category.Type
                ),
                Creator: new CreatorResponse(
                    UserId: t.CreatedBy!.Id,
                    FullName: t.CreatedBy.FullName,
                    ProfileImageUrl: t.CreatedBy.ProfileImageFileId.HasValue
                        ? fileUrlBuilder.GetUrl(t.CreatedBy.ProfileImageFileId.Value)
                        : null
                )
            ))
            .ToListAsync(cancellationToken);

        // Handle empty results
        if (transactions.Count == 0)
        {
            return new CursorPagedResponse<TransactionItem>(
                Items: Array.Empty<TransactionItem>(),
                NextCursor: null,
                HasNextPage: false);
        }

        // Calculate next cursor
        string? nextCursor = transactions.Count == size
            ? transactions.Last().CreatedAtUtc.ToString("o")
            : null;

        // Build response
        return new CursorPagedResponse<TransactionItem>(
            Items: transactions,
            NextCursor: nextCursor,
            HasNextPage: nextCursor is not null);
    }
}
