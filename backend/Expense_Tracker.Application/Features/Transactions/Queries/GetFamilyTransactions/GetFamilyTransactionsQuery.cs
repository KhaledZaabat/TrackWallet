using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Transactions.Queries.GetFamilyTransactions;

public sealed record GetFamilyTransactionsQuery(
    Guid FamilyId,
    int PageSize = 20,
    string? Cursor = null
) : IRequest<Result<CursorPagedResponse<TransactionItem>>>;

public sealed record CursorPagedResponse<T>(
    IReadOnlyList<T> Items,
    string? NextCursor,
    bool HasNextPage
);
