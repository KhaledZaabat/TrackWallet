namespace Expense_Tracker.Contracts.Requests.Identity;

/// <summary>
/// Body for <c>POST /api/identity/reset-password</c>. Carries the magic-link
/// token (taken from the email URL by the SPA) along with the new password.
/// </summary>
public sealed record ResetPasswordRequest(string Email, string Token, string NewPassword);
