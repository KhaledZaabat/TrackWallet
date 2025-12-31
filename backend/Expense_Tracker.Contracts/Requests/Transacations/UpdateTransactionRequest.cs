using Expense_Tracker.Domain.TransactionFolder.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expense_Tracker.Contracts.Requests.Transacations;

public sealed record UpdateTransactionRequest
{
    public TransactionType? Type { get; init; }
    public decimal? Amount { get; init; }
    public DateOnly? TransactedOn { get; init; }
    public string? Title { get; init; }
    public string? Notes { get; init; }
    public Guid? CategoryId { get; init; }
}