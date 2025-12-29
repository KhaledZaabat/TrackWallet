namespace Expense_Tracker.Contracts.Reponses.Identity;

public sealed record TokenResponse(
    string Token,
    DateTime ExpiresAt);
