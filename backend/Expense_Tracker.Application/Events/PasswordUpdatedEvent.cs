namespace Expense_Tracker.Application.Events;

public sealed record PasswordUpdatedEvent(
    string Email,
    string UserName,
    string IpAddress,
    DateTime Timestamp
);
