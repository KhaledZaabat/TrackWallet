using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.TransactionFolder;

namespace Expense_Tracker.Domain.Events;

public sealed record TransactionCreatedEvent(Transaction transaction) : DomainEvent;