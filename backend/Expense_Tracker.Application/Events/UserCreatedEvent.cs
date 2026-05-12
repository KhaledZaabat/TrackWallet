using Expense_Tracker.Domain.Users;

namespace Expense_Tracker.Application.Events;

public sealed record UserCreatedEvent(User User);
