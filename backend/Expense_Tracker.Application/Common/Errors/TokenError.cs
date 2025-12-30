using Expense_Tracker.Domain.Common.ResultPattern.Error;

namespace Expense_Tracker.Application.Common.Errors;

public sealed record TokenError(ApplicationErrorCode ApplicationError, string Type, string Description)
    : Error(ApplicationError, Type, Description)
{
    public static readonly TokenError None =
        new(ApplicationErrorCode.None, "Token.None", string.Empty);

    public static TokenError Invalid(string description = "Invalid or expired token") =>
        new(ApplicationErrorCode.Unauthorized, "Token.Invalid", description);

    public static TokenError Expired(string description = "Token has expired") =>
        new(ApplicationErrorCode.Unauthorized, "Token.Expired", description);

    public static TokenError Tampered(string description = "Token signature invalid") =>
        new(ApplicationErrorCode.Unauthorized, "Token.Tampered", description);

    public static TokenError RefreshInvalid(string description = "Invalid or revoked refresh token") =>
        new(ApplicationErrorCode.Unauthorized, "Token.RefreshInvalid", description);
    public static TokenError Revoked(string description = "Token has been revoked") =>
      new(ApplicationErrorCode.Unauthorized, "Token.Revoked", description);
    public static TokenError NotFound(string description = "Token NotFound") =>
  new(ApplicationErrorCode.NotFound, "Token.NotFound", description);
}
