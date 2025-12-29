

using Expense_Tracker.Domain.Common.ResultPattern.Error;
namespace Expense_Tracker.Application.Common.Errors;

public sealed record ApplicationErrorCode : ErrorCode
{
    private ApplicationErrorCode(string value) : base(value) { }

    public static readonly ApplicationErrorCode Validation =
        new("app.validation");

    public static readonly ApplicationErrorCode Unauthorized =
        new("app.unauthorized");

    public static readonly ApplicationErrorCode Forbidden =
        new("app.forbidden");

    public static readonly ApplicationErrorCode Conflict =
        new("app.conflict");

    public static readonly ApplicationErrorCode BadRequest =
        new("app.bad_request");

    public static readonly ApplicationErrorCode None =
        new("app.none");

    public static readonly ApplicationErrorCode NotFound =
       new("app.not_found");
}