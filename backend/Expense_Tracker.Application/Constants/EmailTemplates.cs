namespace Expense_Tracker.Application.Constants;

public static class EmailTemplates
{

    public const string ForgotPasswordOtp = "ForgotPasswordOtp";
    public const string PasswordUpdatedTemplate = "PasswordUpdated";
    public const string UserCreatedTemplate = "UserCreated";
    public const string ResendConfirmationTemplate = "ResendConfirmation";

    // New invitation templates
    public const string InvitationCreatedTemplate = "InvitationCreated.html";
    public const string InvitationAcceptedTemplate = "InvitationAccepted.html";
    public const string InvitationDeclinedTemplate = "InvitationDeclined.html";
    public const string InvitationCancelledTemplate = "InvitationCancelled.html";

}
