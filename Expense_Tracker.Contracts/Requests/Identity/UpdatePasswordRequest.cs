namespace Expense_Tracker.Contracts.Requests.Identity;

public sealed record UpdatePasswordRequest(string CurrentPassword, string NewPassword);
