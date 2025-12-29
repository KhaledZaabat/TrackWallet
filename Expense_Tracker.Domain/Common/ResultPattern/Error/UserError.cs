namespace Expense_Tracker.Domain.Common.ResultPattern.Error;

public sealed record UserError(DomainErrorCode DomainErrorCode, string Type, string Description)
    : Error(DomainErrorCode, Type, Description)
{
    public static readonly UserError None =
        new(DomainErrorCode.None, "User.None", string.Empty);

    public static UserError NotFound(string description = "User not found") =>
        new(DomainErrorCode.NotFound, "User.NotFound", description);

    public static UserError InvalidState(string description = "User is in an invalid state") =>
        new(DomainErrorCode.InvalidState, "User.InvalidState", description);

    public static UserError AlreadyExists(string description = "User already exists") =>
        new(DomainErrorCode.Conflict, "User.AlreadyExists", description);

    public static UserError InvalidOperation(string description = "Invalid operation on user") =>
        new(DomainErrorCode.Validation, "User.InvalidOperation", description);

    public static UserError InvalidSubmission(string description = "Invalid user data submitted") =>
        new(DomainErrorCode.Validation, "User.InvalidSubmission", description);

    public static UserError UsedHandle(string description = "Invalid Handle it is used before choose another one please") =>
      new(DomainErrorCode.Conflict, "User.UsedHandle", description);


    public static UserError Forbidden(string description = "Access denied") =>
      new(DomainErrorCode.Forbidden, "User.Forbidden", description);


}