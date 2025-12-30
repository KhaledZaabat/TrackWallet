using Expense_Tracker.Domain.TransactionFolder.Enums;

namespace Expense_Tracker.Contracts.Reponses.Transaction;

public sealed record CreateTransactionRequest(
    TransactionType Type,
    Guid CategoryId,
    decimal Amount,
    DateOnly TransactedOn,
    string Title,
    string? Notes
);
