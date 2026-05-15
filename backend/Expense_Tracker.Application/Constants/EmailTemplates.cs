namespace Expense_Tracker.Application.Constants;

/// <summary>
/// Stable identifiers for the HTML templates under
/// <c>Expense_Tracker.Infrastructure/Email/Templates</c>. The template loader
/// appends <c>.html</c> when needed, so values are stored without the
/// extension.
/// </summary>
public static class EmailTemplates
{
    // Account lifecycle
    public const string UserCreatedTemplate = "UserCreated";
    public const string ResendConfirmationTemplate = "ResendConfirmation";
    public const string ResetPasswordTemplate = "ResetPassword";
    public const string PasswordUpdatedTemplate = "PasswordUpdated";

    // Family invitations
    public const string InvitationCreatedTemplate = "InvitationCreated.html";
    public const string InvitationAcceptedTemplate = "InvitationAccepted.html";
    public const string InvitationDeclinedTemplate = "InvitationDeclined.html";
    public const string InvitationCancelledTemplate = "InvitationCancelled.html";
}
