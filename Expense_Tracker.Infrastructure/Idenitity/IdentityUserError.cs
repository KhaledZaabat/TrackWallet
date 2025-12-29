using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Infrastructure.Common.Errors;

namespace Expense_Tracker.Infrastructure.Idenitity
{
    public sealed record IdentityUserError(InfrastructureErrorCode InfrastructureErrorCode, string Type, string Description)
        : Error(InfrastructureErrorCode, Type, Description)
    {
        public static readonly IdentityUserError None =
            new(InfrastructureErrorCode.None, "IdentityUser.None", string.Empty);

        // ---------------------------
        // VALIDATION ERRORS (400)
        // ---------------------------

        public static IdentityUserError InvalidEmail(string description = "Email format is invalid.") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.InvalidEmail", description);

        public static IdentityUserError EmptyEmail(string description = "Email cannot be empty.") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.EmptyEmail", description);

        public static IdentityUserError EmptyFullName(string description = "Full name cannot be empty.") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.EmptyFullName", description);

        public static IdentityUserError InvalidFullName(string description = "Full name format is invalid.") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.InvalidFullName", description);

        public static IdentityUserError Validation(string description) =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.ValidationFailed", description);


        // ---------------------------
        // AUTHENTICATION (401)
        // ---------------------------

        public static IdentityUserError InvalidCredentials(string description = "Invalid email or password") =>
            new(InfrastructureErrorCode.Authentication, "IdentityUser.InvalidCredentials", description);

        public static IdentityUserError PasswordMismatch(string description = "Current password is incorrect") =>
            new(InfrastructureErrorCode.Authentication, "IdentityUser.PasswordMismatch", description);


        // ---------------------------
        // NOT FOUND (404)
        // ---------------------------

        public static IdentityUserError NotFound(string description = "Identity user not found") =>
            new(InfrastructureErrorCode.NotFound, "IdentityUser.NotFound", description);


        // ---------------------------
        // STATE / VALIDATION (400)
        // ---------------------------

        public static IdentityUserError EmailNotConfirmed(string description = "Email not confirmed") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.EmailNotConfirmed", description);

        public static IdentityUserError PhoneNotConfirmed(string description = "Phone number not confirmed") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.PhoneNotConfirmed", description);

        public static IdentityUserError DuplicateEmail(string description = "Email already registered") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.DuplicateEmail", description);

        public static IdentityUserError DuplicatePhone(string description = "Phone number already registered") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.DuplicatePhone", description);

        public static IdentityUserError DuplicatedConfirmation(string description = "User already confirmed") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.DuplicatedConfirmation", description);

        public static IdentityUserError WeakPassword(string description = "Password does not meet requirements") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.WeakPassword", description);

        public static IdentityUserError SamePassword(string description = "New password cannot match the old one") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.SamePassword", description);

        public static IdentityUserError UnverifiedAccount(string description = "User must verify email or phone first") =>
            new(InfrastructureErrorCode.Validation, "IdentityUser.UnverifiedAccount", description);


        // ---------------------------
        // SERVER / DATABASE (500)
        // ---------------------------

        public static IdentityUserError CreationFailed(string description = "Unable to create identity user") =>
            new(InfrastructureErrorCode.Database, "IdentityUser.CreationFailed", description);

        public static IdentityUserError UpdateFailed(string description = "Failed to update identity user") =>
            new(InfrastructureErrorCode.Database, "IdentityUser.UpdateFailed", description);

        public static IdentityUserError Conflict(string description = "Identity user conflict occurred") =>
            new(InfrastructureErrorCode.Database, "IdentityUser.Conflict", description);


        // ---------------------------
        // EXTERNAL SERVICE (502)
        // ---------------------------

        public static IdentityUserError PasswordChangeFailed(string description = "Failed to change password") =>
            new(InfrastructureErrorCode.ExternalService, "IdentityUser.PasswordChangeFailed", description);

        public static IdentityUserError PasswordResetFailed(string description = "Failed to reset password") =>
            new(InfrastructureErrorCode.ExternalService, "IdentityUser.PasswordResetFailed", description);

        public static IdentityUserError OperationFailed(string description = "Identity operation failed") =>
            new(InfrastructureErrorCode.ExternalService, "IdentityUser.OperationFailed", description);




    }
}
