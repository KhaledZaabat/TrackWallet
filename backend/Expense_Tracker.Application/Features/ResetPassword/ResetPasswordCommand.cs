namespace Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;

/// <summary>
/// Atomic reset-password command. Validates the magic-link token and applies
/// the new password in a single Identity round-trip.
/// </summary>
public sealed record ResetPasswordCommand(
    string Email,
    string Token,
    string NewPassword,
    string UserIpAddress);
