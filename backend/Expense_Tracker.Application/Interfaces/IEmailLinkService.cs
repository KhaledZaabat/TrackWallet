namespace Expense_Tracker.Application.Interfaces;

/// <summary>
/// Builds the magic-link URLs that email templates point users at.
/// </summary>
/// <remarks>
/// The implementation reads <c>EmailLinkOptions</c> at the singleton lifetime,
/// so URL construction is allocation-light and works from any context
/// (event handlers, background jobs, Hangfire workers).
/// </remarks>
public interface IEmailLinkService : ISingletonService
{
    /// <summary>
    /// Returns an absolute URL like
    /// <c>https://localhost:4200/auth/confirm?email=alice@x.io&amp;token=...</c>.
    /// </summary>
    string BuildConfirmEmailLink(string email, string token);

    /// <summary>
    /// Returns an absolute URL like
    /// <c>https://localhost:4200/auth/reset-password?email=alice@x.io&amp;token=...</c>.
    /// </summary>
    string BuildResetPasswordLink(string email, string token);
}
