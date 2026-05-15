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
    IFileUrlResolver fileUrlResolver
)
{
    public async Task<ErrorOr<CursorPagedResponse<TransactionItem>>> Handle(
        GetFamilyTransactionsQuery request,
        CancellationToken cancellationToken)
    {
        DateTimeOffset cursor;
        if (string.IsNullOrWhiteSpace(request.Cursor))
        {
            cursor = DateTimeOffset.UtcNow;
        }
        else if (!DateTimeOffset.TryParse(request.Cursor, out cursor))
        {
            cursor = DateTimeOffset.UtcNow;
        }

        int size = request.PageSize is <= 0 or > 50 ? 20 : request.PageSize;

        var query = transactionRepo.Query()
            .Where(t =>
                t.FamilyId == request.FamilyId &&
                t.CreatedAtUtc < cursor);

        if (request.TransactionType.HasValue)
        {
            query = query.Where(t => t.Type == request.TransactionType.Value);
        }

        if (request.CategoryType.HasValue)
        {
            query = query.Where(t => t.Category!.Type == request.CategoryType.Value);
        }

        if (request.MinAmount.HasValue)
        {
            query = query.Where(t => t.Amount >= request.MinAmount.Value);
        }

        if (request.MaxAmount.HasValue)
        {
            query = query.Where(t => t.Amount <= request.MaxAmount.Value);
        }

        if (request.CreatorId.HasValue)
        {
            query = query.Where(t => t.CreatedById == request.CreatorId.Value);
        }

        query = query
            .OrderByDescending(t => t.CreatedAtUtc)
            .ThenByDescending(t => t.Id)
            .Take(size);

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
                        ? fileUrlResolver.GetUrl(t.CreatedBy.ProfileImageFileId.Value)
                        : null
                )
            ))
            .ToListAsync(cancellationToken);

        if (transactions.Count == 0)
        {
            return new CursorPagedResponse<TransactionItem>(
                Items: Array.Empty<TransactionItem>(),
                NextCursor: null,
                HasNextPage: false);
        }

        string? nextCursor = transactions.Count == size
            ? transactions.Last().CreatedAtUtc.ToString("o")
            : null;

        return new CursorPagedResponse<TransactionItem>(
            Items: transactions,
            NextCursor: nextCursor,
            HasNextPage: nextCursor is not null);
    }
}
