namespace Expense_Tracker.Contracts.Requests.Identity;

/// <summary>
/// Body for <c>POST /api/identity/confirm-account</c>. Both fields originate
/// from the magic link the user clicked
/// (<c>/auth/confirm?email=…&amp;token=…</c>).
/// </summary>
public sealed record ConfirmAccountRequest(string Email, string Token);
