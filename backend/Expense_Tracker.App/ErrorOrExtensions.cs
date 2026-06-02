using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Expense_Tracker.App;

/// <summary>
/// Maps <see cref="ErrorOr{T}"/> outcomes onto RFC 7807 problem-details
/// responses. Keeps controllers free of error-shaping boilerplate and gives
/// the SPA one consistent error envelope to consume.
/// </summary>
/// <remarks>
/// Wire shape:
/// <list type="bullet">
///   <item>The error's <c>Description</c> is the user-facing
///   <c>title</c>.</item>
///   <item>The error's <c>Code</c> is exposed as <c>extensions.code</c> for
///   client-side dispatch.</item>
///   <item>Multiple validation errors collapse into the standard
///   <c>errors</c> map; a single non-field validation error is a flat
///   problem so the SPA can show one toast instead of one keyed entry.</item>
/// </list>
/// </remarks>
public static class ErrorOrExtensions
{
    private const string CodeExtensionKey = "code";
    private const string CodesExtensionKey = "codes";

    // Validation codes that aren't tied to a specific input field. We keep
    // these as flat problems instead of forcing them into the errors map.
    private static readonly string[] NonFieldValidationCodePrefixes =
    {
        "Identity.InvalidToken",
        "Identity.DuplicatedConfirmation",
        "Identity.UnverifiedAccount",
        "Otp.",
        "Token.Missing",
    };

    // ----- Controller-style helpers ---------------------------------------

    public static IActionResult ToActionResult<T>(
        this ErrorOr<T> errorOr,
        Func<T, IActionResult> onValue,
        ControllerBase controller
    ) => errorOr.Match(value => onValue(value), errors => controller.Problem(errors));

    public static ActionResult<T> ToActionResult<T>(
        this ErrorOr<T> errorOr,
        ControllerBase controller
    ) => errorOr.Match<ActionResult<T>>(value => value, errors => controller.Problem(errors));

    public static IActionResult ToActionResult(
        this ErrorOr<Success> errorOr,
        ControllerBase controller
    ) => errorOr.Match<IActionResult>(_ => controller.Ok(), errors => controller.Problem(errors));

    public static async Task<IActionResult> ToActionResultAsync<T>(
        this Task<ErrorOr<T>> errorOrTask,
        Func<T, IActionResult> onValue,
        ControllerBase controller
    ) => (await errorOrTask).ToActionResult(onValue, controller);

    public static async Task<ActionResult<T>> ToActionResultAsync<T>(
        this Task<ErrorOr<T>> errorOrTask,
        ControllerBase controller
    ) => (await errorOrTask).ToActionResult(controller);

    public static async Task<IActionResult> ToActionResultAsync(
        this Task<ErrorOr<Success>> errorOrTask,
        ControllerBase controller
    ) => (await errorOrTask).ToActionResult(controller);

    public static ActionResult Problem(this ControllerBase controller, List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return new ObjectResult(
                controller.ProblemDetailsFactory.CreateProblemDetails(controller.HttpContext)
            )
            {
                StatusCode = StatusCodes.Status500InternalServerError,
            };
        }

        // Multiple validation errors that look field-shaped → ValidationProblemDetails.
        if (errors.Count > 1 && errors.All(IsFieldValidation))
            return BuildValidationProblem(controller, errors);

        // Everything else is a flat problem.
        return BuildFlatProblem(controller, errors);
    }

    private static ActionResult BuildValidationProblem(
        ControllerBase controller,
        List<Error> errors
    )
    {
        var modelState = new ModelStateDictionary();
        foreach (Error error in errors)
        {
            // The field name is whatever follows the first "." in the code,
            // lower-cased to match the SPA's form-control naming.
            string field = ToFieldName(error.Code);
            modelState.AddModelError(field, error.Description);
        }

        var problem = controller.ProblemDetailsFactory.CreateValidationProblemDetails(
            controller.HttpContext,
            modelState
        );

        problem.Title = "Some of your input isn't valid. Please check and try again.";
        problem.Extensions[CodesExtensionKey] = errors.Select(e => e.Code).ToArray();

        return new UnprocessableEntityObjectResult(problem);
    }

    private static ActionResult BuildFlatProblem(ControllerBase controller, List<Error> errors)
    {
        Error first = errors[0];
        int status = first.ToHttpStatusCode();

        var problem = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode: status,
            title: first.Description,
            detail: null
        );

        problem.Extensions[CodeExtensionKey] = first.Code;

        return new ObjectResult(problem) { StatusCode = status };
    }

    // ----- Minimal-API helpers --------------------------------------------

    public static IResult ToResult<T>(this ErrorOr<T> errorOr, Func<T, IResult> onValue) =>
        errorOr.Match(value => onValue(value), errors => errors.ToProblemResult());

    public static async Task<IResult> ToResultAsync<T>(
        this Task<ErrorOr<T>> errorOrTask,
        Func<T, IResult> onValue
    ) => (await errorOrTask).ToResult(onValue);

    public static IResult ToProblemResult(this List<Error> errors)
    {
        if (errors.Count == 0)
            return Results.Problem();

        if (errors.Count > 1 && errors.All(IsFieldValidation))
        {
            return Results.ValidationProblem(
                errors
                    .GroupBy(e => ToFieldName(e.Code))
                    .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray()),
                title: "Some of your input isn't valid. Please check and try again.",
                statusCode: StatusCodes.Status422UnprocessableEntity,
                extensions: new Dictionary<string, object?>
                {
                    [CodesExtensionKey] = errors.Select(e => e.Code).ToArray(),
                }
            );
        }

        Error first = errors[0];
        return Results.Problem(
            statusCode: first.ToHttpStatusCode(),
            title: first.Description,
            extensions: new Dictionary<string, object?> { [CodeExtensionKey] = first.Code }
        );
    }

    // ----- Helpers ---------------------------------------------------------

    public static int ToHttpStatusCode(this Error error) =>
        error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Failure => StatusCodes.Status400BadRequest,
            ErrorType.Unexpected => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError,
        };

    public static IEnumerable<string> ToErrorMessages<T>(this ErrorOr<T> errorOr) =>
        errorOr.IsError ? errorOr.Errors.Select(e => e.Description) : Enumerable.Empty<string>();

    public static IEnumerable<string> ToErrorCodes<T>(this ErrorOr<T> errorOr) =>
        errorOr.IsError ? errorOr.Errors.Select(e => e.Code) : Enumerable.Empty<string>();

    public static ErrorOr<IReadOnlyList<T>> Combine<T>(params ErrorOr<T>[] results)
    {
        var errors = results.Where(r => r.IsError).SelectMany(r => r.Errors).ToList();
        if (errors.Count > 0)
            return errors;

        IReadOnlyList<T> values = results.Select(r => r.Value).ToList().AsReadOnly();
        return ErrorOrFactory.From(values);
    }

    public static ErrorOr<T> ToErrorOrNotNull<T>(this T? value, Error error)
        where T : class => value is null ? error : value;

    public static ErrorOr<T> ToErrorOrNotNull<T>(this T? value, Error error)
        where T : struct => value is null ? error : value.Value;

    private static bool IsFieldValidation(Error error) =>
        error.Type == ErrorType.Validation && !IsNonFieldValidation(error.Code);

    private static bool IsNonFieldValidation(string code)
    {
        foreach (string prefix in NonFieldValidationCodePrefixes)
        {
            if (code.StartsWith(prefix, StringComparison.Ordinal))
                return true;
        }
        return false;
    }

    private static string ToFieldName(string code)
    {
        // "Identity.InvalidEmail" → "email"
        int dot = code.IndexOf('.');
        if (dot < 0 || dot == code.Length - 1)
            return code;

        string tail = code[(dot + 1)..];

        // Strip a leading "Invalid" / "Empty" / "Duplicate" prefix if present
        // so "Identity.InvalidEmail" maps to "email" not "invalidEmail".
        foreach (string p in new[] { "Invalid", "Empty", "Duplicate" })
        {
            if (tail.StartsWith(p, StringComparison.Ordinal) && tail.Length > p.Length)
            {
                tail = tail[p.Length..];
                break;
            }
        }

        return char.ToLowerInvariant(tail[0]) + tail[1..];
    }
}
