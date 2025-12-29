using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Infrastructure.Common.Errors;

namespace Expense_Tracker.Infrastructure.Idenitity;

public sealed record RefreshTokenError(InfrastructureErrorCode InfrastructureErrorCode, string Type, string Description)
       : Error(InfrastructureErrorCode, Type, Description)
{
    public static readonly RefreshTokenError None =
        new(InfrastructureErrorCode.None, "RefreshToken.None", string.Empty);

    //  Authentication / Token Issues ---------------------------------------------

    public static RefreshTokenError Invalid(string description = "Invalid refresh token.") =>
        new(InfrastructureErrorCode.Authentication, "RefreshToken.Invalid", description);

    public static RefreshTokenError Expired(string description = "Refresh token has expired.") =>
        new(InfrastructureErrorCode.Authentication, "RefreshToken.Expired", description);

    public static RefreshTokenError Revoked(string description = "Refresh token has been revoked.") =>
        new(InfrastructureErrorCode.Authentication, "RefreshToken.Revoked", description);

    public static RefreshTokenError Inactive(string description = "Refresh token is not active.") =>
        new(InfrastructureErrorCode.Authentication, "RefreshToken.Inactive", description);

    public static RefreshTokenError ReuseDetected(string description = "Attempted reuse of refresh token.") =>
        new(InfrastructureErrorCode.Authentication, "RefreshToken.ReuseDetected", description);

    //  Not Found -------------------------------------------------------------------

    public static RefreshTokenError NotFound(string description = "Refresh token not found.") =>
        new(InfrastructureErrorCode.NotFound, "RefreshToken.NotFound", description);

    //  Validation Issues -----------------------------------------------------------

    public static RefreshTokenError Missing(string description = "Refresh token is missing.") =>
        new(InfrastructureErrorCode.Validation, "RefreshToken.Missing", description);

    public static RefreshTokenError InvalidUser(string description = "User associated with refresh token does not exist.") =>
        new(InfrastructureErrorCode.Validation, "RefreshToken.InvalidUser", description);

    public static RefreshTokenError InvalidState(string description = "Refresh token is in an invalid state.") =>
        new(InfrastructureErrorCode.Validation, "RefreshToken.InvalidState", description);

    //  Database / Infrastructure Issues -------------------------------------------

    public static RefreshTokenError CreationFailed(string description = "Failed to create refresh token.") =>
        new(InfrastructureErrorCode.Database, "RefreshToken.CreationFailed", description);

    public static RefreshTokenError PersistenceFailed(string description = "Failed to persist refresh token.") =>
        new(InfrastructureErrorCode.Database, "RefreshToken.PersistenceFailed", description);

    public static RefreshTokenError UpdateFailed(string description = "Failed to update refresh token.") =>
        new(InfrastructureErrorCode.Database, "RefreshToken.UpdateFailed", description);

    //  Forbidden Usage -------------------------------------------------------------

    public static RefreshTokenError Forbidden(string description = "Refresh token cannot be used.") =>
        new(InfrastructureErrorCode.Forbidden, "RefreshToken.Forbidden", description);

    //  External Services -----------------------------------------------------------

    public static RefreshTokenError ExternalService(string description = "Error while processing refresh token with external service.") =>
        new(InfrastructureErrorCode.ExternalService, "RefreshToken.ExternalService", description);

    // Unexpected -------------------------------------------------------------------

    public static RefreshTokenError Unexpected(string description = "Unexpected refresh token error occurred.") =>
        new(InfrastructureErrorCode.Timeout, "RefreshToken.Unexpected", description);
}