using ErrorOr;

namespace Expense_Tracker.Domain.Errors;

public static class DomainErrors
{
    public static class UserErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("User.NotFound", description ?? "User not found.");

        public static Error InvalidState(string? description = null) =>
            Error.Failure("User.InvalidState", description ?? "User is in an invalid state.");

        public static Error AlreadyExists(string? description = null) =>
            Error.Conflict("User.AlreadyExists", description ?? "User already exists.");

        public static Error InvalidOperation(string? description = null) =>
            Error.Validation("User.InvalidOperation", description ?? "Invalid operation on user.");

        public static Error InvalidSubmission(string? description = null) =>
            Error.Validation(
                "User.InvalidSubmission",
                description ?? "Invalid user data submitted."
            );

        public static Error UsedHandle(string? description = null) =>
            Error.Conflict("User.UsedHandle", description ?? "Handle is already in use.");

        public static Error Forbidden(string? description = null) =>
            Error.Forbidden("User.Forbidden", description ?? "Access denied.");

        public static Error Unauthorized(string? description = null) =>
            Error.Unauthorized("User.Unauthorized", description ?? "User is not authorized.");
    }

    public static class FamilyErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("Family.NotFound", description ?? "Family not found.");

        public static Error InvalidState(string? description = null) =>
            Error.Failure("Family.InvalidState", description ?? "Family is in an invalid state.");

        public static Error AlreadyExists(string? description = null) =>
            Error.Conflict("Family.AlreadyExists", description ?? "Family already exists.");
    }

    public static class TransactionErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("Transaction.NotFound", description ?? "Transaction not found.");

        public static Error InvalidAmount(decimal amount) =>
            Error.Validation("Transaction.InvalidAmount", $"Amount {amount:C} is invalid.");

        public static Error BudgetNotEnough(decimal current, decimal requested) =>
            Error.Validation(
                "Transaction.BudgetNotEnough",
                $"Budget {current:C} is insufficient for {requested:C}."
            );
    }

    public static class CategoryErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("Category.NotFound", description ?? "Category not found.");
    }

    public static class InvitationErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("Invitation.NotFound", description ?? "Invitation not found.");

        public static Error Forbidden(string? description = null) =>
            Error.Forbidden(
                "Invitation.Forbidden",
                description ?? "You are not allowed to perform this action on the invitation."
            );

        public static Error AlreadyAccepted(string? description = null) =>
            Error.Failure(
                "Invitation.AlreadyAccepted",
                description ?? "Invitation already accepted."
            );

        public static Error AlreadyDeclined(string? description = null) =>
            Error.Failure(
                "Invitation.AlreadyDeclined",
                description ?? "Invitation was declined and cannot be accepted."
            );

        public static Error Cancelled(string? description = null) =>
            Error.Failure("Invitation.Cancelled", description ?? "Invitation was cancelled.");

        public static Error NotPending(string? description = null) =>
            Error.Failure(
                "Invitation.NotPending",
                description ?? "Only pending invitations can be cancelled."
            );

        public static Error SelfInvite(string? description = null) =>
            Error.Validation("Invitation.SelfInvite", description ?? "Cannot invite yourself.");
    }

    public static class TokenErrors
    {
        public static Error Invalid(string? description = null) =>
            Error.Unauthorized("Token.Invalid", description ?? "Invalid or expired token.");

        public static Error Expired(string? description = null) =>
            Error.Unauthorized("Token.Expired", description ?? "Token has expired.");

        public static Error Tampered(string? description = null) =>
            Error.Unauthorized("Token.Tampered", description ?? "Token signature invalid.");

        public static Error RefreshInvalid(string? description = null) =>
            Error.Unauthorized(
                "Token.RefreshInvalid",
                description ?? "Invalid or revoked refresh token."
            );

        public static Error Revoked(string? description = null) =>
            Error.Unauthorized("Token.Revoked", description ?? "Token has been revoked.");

        public static Error NotFound(string? description = null) =>
            Error.NotFound("Token.NotFound", description ?? "Token not found.");

        public static Error Missing(string? description = null) =>
            Error.Validation("Token.Missing", description ?? "Token is missing.");

        public static Error Inactive(string? description = null) =>
            Error.Unauthorized("Token.Inactive", description ?? "Token is not active.");

        public static Error ReuseDetected(string? description = null) =>
            Error.Unauthorized("Token.ReuseDetected", description ?? "Attempted reuse of token.");

        public static Error Forbidden(string? description = null) =>
            Error.Forbidden("Token.Forbidden", description ?? "Token cannot be used.");

        public static Error CreationFailed(string? description = null) =>
            Error.Unexpected("Token.CreationFailed", description ?? "Failed to create token.");

        public static Error PersistenceFailed(string? description = null) =>
            Error.Unexpected("Token.PersistenceFailed", description ?? "Failed to persist token.");

        public static Error UpdateFailed(string? description = null) =>
            Error.Unexpected("Token.UpdateFailed", description ?? "Failed to update token.");

        public static Error Conflict(string? description = null) =>
            Error.Conflict("Token.Conflict", description ?? "A token conflict occurred.");
    }

    public static class OtpErrors
    {
        public static Error InvalidOrExpired(string? description = null) =>
            Error.Validation("Otp.InvalidOrExpired", description ?? "Invalid or expired OTP code.");

        public static Error NotExpired(string? description = null) =>
            Error.Conflict(
                "Otp.NotExpired",
                description
                    ?? "An active OTP already exists. Please wait before requesting a new one."
            );
    }

    public static class FileErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("File.NotFound", description ?? "File not found.");

        public static Error InvalidExtension(string? description = null) =>
            Error.Validation("File.InvalidExtension", description ?? "Invalid file extension.");

        public static Error TooLarge(string? description = null) =>
            Error.Validation("File.TooLarge", description ?? "File is too large.");

        public static Error Empty(string? description = null) =>
            Error.Validation("File.Empty", description ?? "File is empty.");

        public static Error InvalidType(string? description = null) =>
            Error.Validation("File.InvalidType", description ?? "Invalid file type.");

        public static Error UploadFailed(string? description = null) =>
            Error.Failure("File.UploadFailed", description ?? "Failed to upload file.");

        public static Error DownloadFailed(string? description = null) =>
            Error.Failure("File.DownloadFailed", description ?? "Failed to download file.");

        public static Error StreamFailed(string? description = null) =>
            Error.Failure("File.StreamFailed", description ?? "Failed to stream file.");

        public static Error ValidationFailed(string? description = null) =>
            Error.Validation("File.ValidationFailed", description ?? "File validation failed.");
    }

    public static class IdentityErrors
    {
        public static Error InvalidEmail(string? description = null) =>
            Error.Validation("Identity.InvalidEmail", description ?? "Email format is invalid.");

        public static Error EmptyEmail(string? description = null) =>
            Error.Validation("Identity.EmptyEmail", description ?? "Email cannot be empty.");

        public static Error EmptyFullName(string? description = null) =>
            Error.Validation("Identity.EmptyFullName", description ?? "Full name cannot be empty.");

        public static Error InvalidFullName(string? description = null) =>
            Error.Validation(
                "Identity.InvalidFullName",
                description ?? "Full name format is invalid."
            );

        public static Error InvalidCredentials(string? description = null) =>
            Error.Unauthorized(
                "Identity.InvalidCredentials",
                description ?? "Invalid email or password."
            );

        public static Error PasswordMismatch(string? description = null) =>
            Error.Unauthorized(
                "Identity.PasswordMismatch",
                description ?? "Current password is incorrect."
            );

        public static Error NotFound(string? description = null) =>
            Error.NotFound("Identity.NotFound", description ?? "Identity user not found.");

        public static Error EmailNotConfirmed(string? description = null) =>
            Error.Validation("Identity.EmailNotConfirmed", description ?? "Email not confirmed.");

        public static Error PhoneNotConfirmed(string? description = null) =>
            Error.Validation(
                "Identity.PhoneNotConfirmed",
                description ?? "Phone number not confirmed."
            );

        public static Error DuplicateEmail(string? description = null) =>
            Error.Conflict("Identity.DuplicateEmail", description ?? "Email already registered.");

        public static Error DuplicatePhone(string? description = null) =>
            Error.Conflict(
                "Identity.DuplicatePhone",
                description ?? "Phone number already registered."
            );

        public static Error DuplicatedConfirmation(string? description = null) =>
            Error.Validation(
                "Identity.DuplicatedConfirmation",
                description ?? "User already confirmed."
            );

        public static Error WeakPassword(string? description = null) =>
            Error.Validation(
                "Identity.WeakPassword",
                description ?? "Password does not meet requirements."
            );

        public static Error SamePassword(string? description = null) =>
            Error.Validation(
                "Identity.SamePassword",
                description ?? "New password cannot match the old one."
            );

        public static Error UnverifiedAccount(string? description = null) =>
            Error.Validation(
                "Identity.UnverifiedAccount",
                description ?? "User must verify email or phone first."
            );

        public static Error CreationFailed(string? description = null) =>
            Error.Unexpected(
                "Identity.CreationFailed",
                description ?? "Unable to create identity user."
            );

        public static Error UpdateFailed(string? description = null) =>
            Error.Unexpected(
                "Identity.UpdateFailed",
                description ?? "Failed to update identity user."
            );

        public static Error PasswordChangeFailed(string? description = null) =>
            Error.Failure(
                "Identity.PasswordChangeFailed",
                description ?? "Failed to change password."
            );

        public static Error PasswordResetFailed(string? description = null) =>
            Error.Failure(
                "Identity.PasswordResetFailed",
                description ?? "Failed to reset password."
            );

        public static Error OperationFailed(string? description = null) =>
            Error.Failure("Identity.OperationFailed", description ?? "Identity operation failed.");

        public static Error RoleNotFound(string? description = null) =>
            Error.NotFound("Identity.RoleNotFound", description ?? "Role not found.");

        public static Error DuplicateRole(string? description = null) =>
            Error.Conflict(
                "Identity.DuplicateRole",
                description ?? "Another role with the same name already exists."
            );

        public static Error InvalidPermissions(string? description = null) =>
            Error.Validation("Identity.InvalidPermissions", description ?? "Invalid permissions.");
    }

    public static class ExternalAuthErrors
    {
        public static Error ProviderUnavailable(string? description = null) =>
            Error.Failure(
                "ExternalAuth.ProviderUnavailable",
                description ?? "Authentication provider is unavailable."
            );

        public static Error InvalidProviderResponse(string? description = null) =>
            Error.Failure(
                "ExternalAuth.InvalidProviderResponse",
                description ?? "Invalid response from provider."
            );

        public static Error UserCreationFailed(string? description = null) =>
            Error.Failure(
                "ExternalAuth.UserCreationFailed",
                description ?? "Failed to create user account."
            );

        public static Error LoginLinkFailed(string? description = null) =>
            Error.Failure(
                "ExternalAuth.LoginLinkFailed",
                description ?? "Failed to link login provider."
            );

        public static Error UserNotRegistered(string? description = null) =>
            Error.NotFound("ExternalAuth.UserNotRegistered", description ?? "User not registered.");

        public static Error Unknown(string? description = null) =>
            Error.Unexpected(
                "ExternalAuth.Unknown",
                description ?? "An unexpected external auth error occurred."
            );
    }

    public static class NotificationErrors
    {
        public static Error NotFound(string? description = null) =>
            Error.NotFound("Notification.NotFound", description ?? "Notification not found.");

        public static Error InvalidState(string? description = null) =>
            Error.Failure(
                "Notification.InvalidState",
                description ?? "Notification is in an invalid state."
            );
    }

    public static class SystemErrors
    {
        public static Error Database(string? description = null) =>
            Error.Unexpected("System.Database", description ?? "A database error occurred.");

        public static Error Timeout(string? description = null) =>
            Error.Unexpected("System.Timeout", description ?? "Operation timed out.");

        public static Error ExternalService(string? description = null) =>
            Error.Unexpected("System.ExternalService", description ?? "External service error.");
    }

    public static class GeneralErrors
    {
        public static Error NotFound(string entity, string? description = null) =>
            Error.NotFound($"{entity}.NotFound", description ?? $"{entity} not found.");

        public static Error Conflict(string entity, string? description = null) =>
            Error.Conflict($"{entity}.Conflict", description ?? $"{entity} already exists.");

        public static Error InvalidState(string entity, string? description = null) =>
            Error.Failure(
                $"{entity}.InvalidState",
                description ?? $"{entity} is in an invalid state."
            );

        public static Error Forbidden(string? description = null) =>
            Error.Forbidden("General.Forbidden", description ?? "Access denied.");

        public static Error Unauthorized(string? description = null) =>
            Error.Unauthorized("General.Unauthorized", description ?? "Unauthorized.");

        public static Error Validation(string? description = null) =>
            Error.Validation("General.Validation", description ?? "Validation failed.");

        public static Error Unexpected(string? description = null) =>
            Error.Unexpected("General.Unexpected", description ?? "An unexpected error occurred.");

        public static Error BusinessRule(string entity, string description) =>
            Error.Validation($"{entity}.BusinessRule", description);
    }
}
