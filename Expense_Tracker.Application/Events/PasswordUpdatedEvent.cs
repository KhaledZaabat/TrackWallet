using Expense_Tracker.Application.Common;

namespace Expense_Tracker.Application.Events;

public sealed record PasswordUpdatedEvent(
    string Email,
    string FullName,
    string IpAddress,
    DateTime Timestamp
) : ApplicationEvent;
