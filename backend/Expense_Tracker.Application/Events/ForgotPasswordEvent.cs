namespace Expense_Tracker.Application.Events;

public sealed record ForgotPasswordEvent(string Email, string UserName);
