using Expense_Tracker.Contracts.Reponses.Category;
using Expense_Tracker.Domain.TransactionFolder.Enums;

namespace Expense_Tracker.Contracts.Reponses.Transaction;

public sealed record TransactionItem(
    Guid TransactionId,
    string Title,
    decimal Amount,
    TransactionType Type,
    DateOnly TransactedOn,
    DateTimeOffset CreatedAtUtc,
    CategoryResponse Category,
    CreatorResponse Creator
);
