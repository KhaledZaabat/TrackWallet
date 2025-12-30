using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Users;

namespace Expense_Tracker.Domain.Events;

public sealed record UserCreatedEvent(User User) : DomainEvent;
