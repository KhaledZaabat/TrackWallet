namespace Expense_Tracker.Contracts.Requests.Identity;

public sealed record ResetPasswordRequest(string Email, string NewPassword);
