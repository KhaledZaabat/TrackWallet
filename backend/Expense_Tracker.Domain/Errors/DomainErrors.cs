using ErrorOr;

namespace Expense_Tracker.Domain.Errors;

/// <summary>
/// Single source of truth for every error the API can return.
/// </summary>
/// <remarks>
/// Descriptions are user-facing — they end up as the <c>title</c> field of
/// the RFC 7807 problem-details response and are surfaced in the SPA's
/// toaster as-is. Keep them concise, in plain English, and free of internal
/// jargon ("persistence", "stamp", "tampered", etc.).
///
/// Codes are machine identifiers in <c>Domain.Subject</c> form. The SPA can
/// dispatch on them when it needs different UX for specific cases.
/// </remarks>
public static class DomainErrors
{
    public static class UserErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("User.NotFound", description ?? "We couldn't find that user.");

        public static Error InvalidState(string? description = null) =>
            Error.Failure(
                "User.InvalidState",
                description ?? "This account isn't in a state we can use right now."
            );

        public static Error AlreadyExists(string? description = null) =>
            Error.Conflict(
                "User.AlreadyExists",
                description ?? "An account already exists for this user."
            );

        public static Error InvalidOperation(string? description = null) =>
            Error.Validation(
                "User.InvalidOperation",
                description ?? "That action isn't allowed for this account."
            );

        public static Error InvalidSubmission(string? description = null) =>
            Error.Validation(
                "User.InvalidSubmission",
                description ?? "Some of the details you entered aren't valid."
            );

        public static Error UsedHandle(string? description = null) =>
            Error.Conflict("User.UsedHandle", description ?? "That username is already taken.");

        public static Error Forbidden(string? description = null) =>
            Error.Forbidden(
                "User.Forbidden",
                description ?? "You don't have permission to do that."
            );

        public static Error Unauthorized(string? description = null) =>
            Error.Unauthorized("User.Unauthorized", description ?? "Please sign in to continue.");
    }

    public static class FamilyErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("Family.NotFound", description ?? "We couldn't find that family.");

        public static Error InvalidState(string? description = null) =>
            Error.Failure(
                "Family.InvalidState",
                description ?? "This family isn't in a state we can use right now."
            );

        public static Error AlreadyExists(string? description = null) =>
            Error.Conflict(
                "Family.AlreadyExists",
                description ?? "A family with that name already exists."
            );
    }

    public static class TransactionErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound(
                "Transaction.NotFound",
                description ?? "We couldn't find that transaction."
            );

        public static Error InvalidAmount(decimal amount) =>
            Error.Validation("Transaction.InvalidAmount", $"The amount {amount:C} isn't valid.");

        public static Error BudgetNotEnough(decimal current, decimal requested) =>
            Error.Validation(
                "Transaction.BudgetNotEnough",
                $"Your remaining budget ({current:C}) isn't enough for {requested:C}."
            );
    }

    public static class CategoryErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("Category.NotFound", description ?? "We couldn't find that category.");
    }

    public static class InvitationErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound(
                "Invitation.NotFound",
                description ?? "We couldn't find that invitation."
            );

        public static Error Forbidden(string? description = null) =>
            Error.Forbidden(
                "Invitation.Forbidden",
                description ?? "You can't act on this invitation."
            );

        public static Error AlreadyAccepted(string? description = null) =>
            Error.Failure(
                "Invitation.AlreadyAccepted",
                description ?? "This invitation has already been accepted."
            );

        public static Error AlreadyDeclined(string? description = null) =>
            Error.Failure(
                "Invitation.AlreadyDeclined",
                description ?? "This invitation was declined and can no longer be used."
            );

        public static Error Cancelled(string? description = null) =>
            Error.Failure("Invitation.Cancelled", description ?? "This invitation was cancelled.");

        public static Error NotPending(string? description = null) =>
            Error.Failure(
                "Invitation.NotPending",
                description ?? "Only pending invitations can be cancelled."
            );

        public static Error SelfInvite(string? description = null) =>
            Error.Validation("Invitation.SelfInvite", description ?? "You can't invite yourself.");
    }

    public static class TokenErrors
    {
        // For tokens we deliberately collapse most internal cases ("Tampered",
        // "Inactive", "ReuseDetected") into the same user-facing message —
        // the user can't act on the difference, only the logs care.
        private const string SessionInvalid =
            "Your session is no longer valid. Please sign in again.";
        private const string SessionExpired = "Your session has expired. Please sign in again.";

        public static Error Invalid(string? description = null) =>
            Error.Unauthorized("Token.Invalid", description ?? SessionInvalid);

        public static Error Expired(string? description = null) =>
            Error.Unauthorized("Token.Expired", description ?? SessionExpired);

        public static Error Tampered(string? description = null) =>
            Error.Unauthorized("Token.Tampered", description ?? SessionInvalid);

        public static Error RefreshInvalid(string? description = null) =>
            Error.Unauthorized("Token.RefreshInvalid", description ?? SessionExpired);

        public static Error Revoked(string? description = null) =>
            Error.Unauthorized(
                "Token.Revoked",
                description ?? "Your session was ended. Please sign in again."
            );

        public static Error NotFound(string? description = null) =>
            Error.NotFound("Token.NotFound", description ?? SessionInvalid);

        public static Error Missing(string? description = null) =>
            Error.Validation("Token.Missing", description ?? "Please sign in to continue.");

        public static Error Inactive(string? description = null) =>
            Error.Unauthorized("Token.Inactive", description ?? SessionInvalid);

        public static Error ReuseDetected(string? description = null) =>
            Error.Unauthorized(
                "Token.ReuseDetected",
                description ?? "Your session was ended for your security. Please sign in again."
            );

        public static Error Forbidden(string? description = null) =>
            Error.Forbidden("Token.Forbidden", description ?? SessionInvalid);

        public static Error CreationFailed(string? description = null) =>
            Error.Unexpected(
                "Token.CreationFailed",
                description ?? "We couldn't start your session. Please try again."
            );

        public static Error PersistenceFailed(string? description = null) =>
            Error.Unexpected(
                "Token.PersistenceFailed",
                description ?? "We couldn't save your session. Please try again."
            );

        public static Error UpdateFailed(string? description = null) =>
            Error.Unexpected(
                "Token.UpdateFailed",
                description ?? "We couldn't update your session. Please try again."
            );

        public static Error Conflict(string? description = null) =>
            Error.Conflict("Token.Conflict", description ?? SessionInvalid);
    }

    public static class OtpErrors
    {
        public static Error InvalidOrExpired(string? description = null) =>
            Error.Validation(
                "Otp.InvalidOrExpired",
                description ?? "This code is invalid or has expired."
            );

        public static Error NotExpired(string? description = null) =>
            Error.Conflict(
                "Otp.NotExpired",
                description ?? "Please wait a moment before requesting a new code."
            );
    }

    public static class FileErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("File.NotFound", description ?? "We couldn't find that file.");

        public static Error InvalidExtension(string? description = null) =>
            Error.Validation(
                "File.InvalidExtension",
                description ?? "That file type isn't supported."
            );

        public static Error TooLarge(string? description = null) =>
            Error.Validation("File.TooLarge", description ?? "That file is too large.");

        public static Error Empty(string? description = null) =>
            Error.Validation("File.Empty", description ?? "Please choose a file to upload.");

        public static Error InvalidType(string? description = null) =>
            Error.Validation("File.InvalidType", description ?? "That file type isn't supported.");

        public static Error UploadFailed(string? description = null) =>
            Error.Failure(
                "File.UploadFailed",
                description ?? "We couldn't upload that file. Please try again."
            );

        public static Error DownloadFailed(string? description = null) =>
            Error.Failure(
                "File.DownloadFailed",
                description ?? "We couldn't download that file. Please try again."
            );

        public static Error StreamFailed(string? description = null) =>
            Error.Failure(
                "File.StreamFailed",
                description ?? "We couldn't load that file. Please try again."
            );

        public static Error ValidationFailed(string? description = null) =>
            Error.Validation(
                "File.ValidationFailed",
                description ?? "That file didn't pass our checks."
            );
    }

    public static class IdentityErrors
    {
        public static Error InvalidEmail(string? description = null) =>
            Error.Validation(
                "Identity.InvalidEmail",
                description ?? "Please enter a valid email address."
            );

        public static Error EmptyEmail(string? description = null) =>
            Error.Validation("Identity.EmptyEmail", description ?? "Email is required.");

        public static Error EmptyFullName(string? description = null) =>
            Error.Validation("Identity.EmptyFullName", description ?? "Full name is required.");

        public static Error InvalidFullName(string? description = null) =>
            Error.Validation(
                "Identity.InvalidFullName",
                description ?? "Please enter a valid full name."
            );

        public static Error InvalidCredentials(string? description = null) =>
            Error.Unauthorized(
                "Identity.InvalidCredentials",
                description ?? "Invalid email or password."
            );

        public static Error PasswordMismatch(string? description = null) =>
            Error.Unauthorized(
                "Identity.PasswordMismatch",
                description ?? "Your current password is incorrect."
            );

        public static Error NotFound(string? description = null) =>
            Error.NotFound(
                "Identity.NotFound",
                description ?? "We couldn't find an account with those details."
            );

        public static Error EmailNotConfirmed(string? description = null) =>
            Error.Validation(
                "Identity.EmailNotConfirmed",
                description ?? "Please confirm your email before signing in."
            );

        public static Error PhoneNotConfirmed(string? description = null) =>
            Error.Validation(
                "Identity.PhoneNotConfirmed",
                description ?? "Please confirm your phone number before signing in."
            );

        public static Error DuplicateEmail(string? description = null) =>
            Error.Conflict(
                "Identity.DuplicateEmail",
                description ?? "An account with this email already exists."
            );

        public static Error DuplicatePhone(string? description = null) =>
            Error.Conflict(
                "Identity.DuplicatePhone",
                description ?? "An account with this phone number already exists."
            );

        public static Error DuplicatedConfirmation(string? description = null) =>
            Error.Validation(
                "Identity.DuplicatedConfirmation",
                description ?? "Your account is already confirmed."
            );

        public static Error WeakPassword(string? description = null) =>
            Error.Validation(
                "Identity.WeakPassword",
                description ?? "Your password doesn't meet the security requirements."
            );

        public static Error SamePassword(string? description = null) =>
            Error.Validation(
                "Identity.SamePassword",
                description ?? "Your new password can't be the same as your current one."
            );

        public static Error UnverifiedAccount(string? description = null) =>
            Error.Validation(
                "Identity.UnverifiedAccount",
                description ?? "Please verify your email or phone first."
            );

        public static Error CreationFailed(string? description = null) =>
            Error.Unexpected(
                "Identity.CreationFailed",
                description ?? "We couldn't create your account. Please try again."
            );

        public static Error UpdateFailed(string? description = null) =>
            Error.Unexpected(
                "Identity.UpdateFailed",
                description ?? "We couldn't update your account. Please try again."
            );

        public static Error PasswordChangeFailed(string? description = null) =>
            Error.Failure(
                "Identity.PasswordChangeFailed",
                description ?? "We couldn't change your password. Please try again."
            );

        public static Error PasswordResetFailed(string? description = null) =>
            Error.Failure(
                "Identity.PasswordResetFailed",
                description ?? "We couldn't reset your password. Please try again."
            );

        public static Error InvalidToken(string? description = null) =>
            Error.Validation(
                "Identity.InvalidToken",
                description ?? "This link is invalid or has expired. Please request a new one."
            );

        public static Error OperationFailed(string? description = null) =>
            Error.Failure(
                "Identity.OperationFailed",
                description ?? "That action couldn't be completed. Please try again."
            );

        public static Error RoleNotFound(string? description = null) =>
            Error.NotFound("Identity.RoleNotFound", description ?? "We couldn't find that role.");

        public static Error DuplicateRole(string? description = null) =>
            Error.Conflict(
                "Identity.DuplicateRole",
                description ?? "A role with that name already exists."
            );

        public static Error InvalidPermissions(string? description = null) =>
            Error.Validation(
                "Identity.InvalidPermissions",
                description ?? "Those permissions aren't valid."
            );
    }

    public static class ExternalAuthErrors
    {
        public static Error ProviderUnavailable(string? description = null) =>
            Error.Failure(
                "ExternalAuth.ProviderUnavailable",
                description ?? "The sign-in provider isn't available right now. Please try again."
            );

        public static Error InvalidProviderResponse(string? description = null) =>
            Error.Failure(
                "ExternalAuth.InvalidProviderResponse",
                description ?? "We couldn't sign you in. Please try again."
            );

        public static Error UserCreationFailed(string? description = null) =>
            Error.Failure(
                "ExternalAuth.UserCreationFailed",
                description ?? "We couldn't create your account. Please try again."
            );

        public static Error LoginLinkFailed(string? description = null) =>
            Error.Failure(
                "ExternalAuth.LoginLinkFailed",
                description ?? "We couldn't link your sign-in. Please try again."
            );

        public static Error UserNotRegistered(string? description = null) =>
            Error.NotFound(
                "ExternalAuth.UserNotRegistered",
                description ?? "We couldn't find an account for this sign-in."
            );

        public static Error Unknown(string? description = null) =>
            Error.Unexpected(
                "ExternalAuth.Unknown",
                description ?? "Something went wrong while signing you in. Please try again."
            );
    }

    public static class NotificationErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound(
                "Notification.NotFound",
                description ?? "We couldn't find that notification."
            );

        public static Error InvalidState(string? description = null) =>
            Error.Failure(
                "Notification.InvalidState",
                description ?? "This notification can't be updated."
            );
    }

    public static class SystemErrors
    {
        public static Error Database(string? description = null) =>
            Error.Unexpected(
                "System.Database",
                description ?? "Something went wrong. Please try again."
            );

        public static Error Timeout(string? description = null) =>
            Error.Unexpected(
                "System.Timeout",
                description ?? "The request took too long. Please try again."
            );

        public static Error ExternalService(string? description = null) =>
            Error.Unexpected(
                "System.ExternalService",
                description ?? "An external service is unavailable. Please try again."
            );
    }

    public static class GeneralErrors
    {
        public static Error NotFound(string entity, string? description = null) =>
            Error.NotFound(
                $"{entity}.NotFound",
                description ?? $"We couldn't find that {entity.ToLowerInvariant()}."
            );

        public static Error Conflict(string entity, string? description = null) =>
            Error.Conflict(
                $"{entity}.Conflict",
                description ?? $"That {entity.ToLowerInvariant()} already exists."
            );

        public static Error InvalidState(string entity, string? description = null) =>
            Error.Failure(
                $"{entity}.InvalidState",
                description
                    ?? $"This {entity.ToLowerInvariant()} isn't in a state we can use right now."
            );

        public static Error Forbidden(string? description = null) =>
            Error.Forbidden(
                "General.Forbidden",
                description ?? "You don't have permission to do that."
            );

        public static Error Unauthorized(string? description = null) =>
            Error.Unauthorized(
                "General.Unauthorized",
                description ?? "Please sign in to continue."
            );

        public static Error Validation(string? description = null) =>
            Error.Validation(
                "General.Validation",
                description ?? "Some of your input isn't valid. Please check and try again."
            );

        public static Error Unexpected(string? description = null) =>
            Error.Unexpected(
                "General.Unexpected",
                description ?? "Something went wrong. Please try again."
            );

        public static Error BusinessRule(string entity, string description) =>
            Error.Validation($"{entity}.BusinessRule", description);
    }
}
