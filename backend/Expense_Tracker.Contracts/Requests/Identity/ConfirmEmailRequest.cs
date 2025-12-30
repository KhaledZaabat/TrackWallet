namespace Expense_Tracker.Contracts.Requests.Identity;

public sealed record ConfirmAccountRequest(string Email, string Otp);
