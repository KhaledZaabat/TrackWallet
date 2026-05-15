namespace Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;

/// <summary>
/// Confirms a freshly-registered email using the magic-link token. Both
/// <c>Email</c> and <c>Token</c> arrive from the SPA route's query string
/// (<c>/auth/confirm?email=…&amp;token=…</c>) — the same pair we minted on the
/// outbound email.
/// </summary>
public sealed record ConfirmAccountCommand(string Email, string Token);
