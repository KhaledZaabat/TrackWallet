using Expense_Tracker.Contracts.Reponses.Transaction;
using Expense_Tracker.Domain.TransactionFolder.Enums;

namespace Expense_Tracker.Application.Features.Transactions.Commands.CreateTransaction;

public sealed record CreateTransactionCommand(
    Guid UserId,
    Guid FamilyId,
    TransactionType Type,
    decimal Amount,
    DateOnly TransactedOn,
    string Title,
    string? Notes,
    Guid CategoryId
);
