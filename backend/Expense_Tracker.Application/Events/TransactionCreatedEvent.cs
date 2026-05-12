using Expense_Tracker.Domain.TransactionFolder;

namespace Expense_Tracker.Application.Events;

public sealed record TransactionCreatedEvent(Transaction Transaction);
