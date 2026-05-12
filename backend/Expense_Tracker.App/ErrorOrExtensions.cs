using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Expense_Tracker.App
{
    /// <summary>
    ///  Maps an <see cref="ErrorOr{T}"/> to an <see cref="IActionResult"/> or
    ///  <see cref="IResult"/> (Minimal API), translating errors to the correct
    ///  HTTP status codes using RFC 7807 ProblemDetails.
    /// </summary>
    public static class ErrorOrExtensions
    {
        /// <summary>
        ///  Returns the mapped value via <paramref name="onValue"/>, or a
        ///  ProblemDetails response on error.
        /// </summary>
        public static IActionResult ToActionResult<T>(
            this ErrorOr<T> errorOr,
            Func<T, IActionResult> onValue,
            ControllerBase controller
        )
        {
            return errorOr.Match(value => onValue(value), errors => controller.Problem(errors));
        }

        /// <summary>
        ///  Returns Ok(value) on success, or ProblemDetails on error.
        /// </summary>
        public static ActionResult<T> ToActionResult<T>(
            this ErrorOr<T> errorOr,
            ControllerBase controller
        )
        {
            return errorOr.Match<ActionResult<T>>(
                value => value,
                errors => controller.Problem(errors)
            );
        }

        /// <summary>
        ///  Returns Ok() on success, or ProblemDetails on error.
        /// </summary>
        public static IActionResult ToActionResult(
            this ErrorOr<Success> errorOr,
            ControllerBase controller
        )
        {
            return errorOr.Match<IActionResult>(
                _ => controller.Ok(),
                errors => controller.Problem(errors)
            );
        }

        /// <summary>
        ///  Async overload of <see cref="ToActionResult{T}"/>.
        /// </summary>
        public static async Task<IActionResult> ToActionResultAsync<T>(
            this Task<ErrorOr<T>> errorOrTask,
            Func<T, IActionResult> onValue,
            ControllerBase controller
        )
        {
            var errorOr = await errorOrTask;
            return errorOr.ToActionResult(onValue, controller);
        }

        /// <summary>
        ///  Async overload that returns Ok(value) on success.
        /// </summary>
        public static async Task<ActionResult<T>> ToActionResultAsync<T>(
            this Task<ErrorOr<T>> errorOrTask,
            ControllerBase controller
        )
        {
            var errorOr = await errorOrTask;
            return errorOr.ToActionResult(controller);
        }

        /// <summary>
        ///  Async overload for ErrorOr{Success}.
        /// </summary>
        public static async Task<IActionResult> ToActionResultAsync(
            this Task<ErrorOr<Success>> errorOrTask,
            ControllerBase controller
        )
        {
            var errorOr = await errorOrTask;
            return errorOr.ToActionResult(controller);
        }

        /// <summary>
        ///  Returns a ProblemDetails response from a list of <see cref="Error"/>s.
        /// </summary>
        public static ActionResult Problem(this ControllerBase controller, List<Error> errors)
        {
            if (errors.Count is 0)
                return new ObjectResult(
                    controller.ProblemDetailsFactory.CreateProblemDetails(controller.HttpContext)
                )
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                };

            // Surface validation errors as a 422 ValidationProblem
            if (errors.All(e => e.Type == ErrorType.Validation))
            {
                var modelState = new ModelStateDictionary();
                foreach (var error in errors)
                    modelState.AddModelError(error.Code, error.Description);

                return new UnprocessableEntityObjectResult(
                    new ValidationProblemDetails(modelState)
                );
            }

            // For all other errors use the first one to pick the status code
            var first = errors[0];

            return new ObjectResult(
                controller.ProblemDetailsFactory.CreateProblemDetails(
                    controller.HttpContext,
                    statusCode: first.ToHttpStatusCode(),
                    title: first.Code,
                    detail: first.Description
                )
            )
            {
                StatusCode = first.ToHttpStatusCode(),
            };
        }

        /// <summary>
        ///  Returns the mapped value via <paramref name="onValue"/>, or a
        ///  ProblemDetails <see cref="IResult"/> on error.
        /// </summary>
        public static IResult ToResult<T>(this ErrorOr<T> errorOr, Func<T, IResult> onValue)
        {
            return errorOr.Match(value => onValue(value), errors => errors.ToProblemResult());
        }

        /// <summary>
        ///  Async overload of <see cref="ToResult{T}"/>.
        /// </summary>
        public static async Task<IResult> ToResultAsync<T>(
            this Task<ErrorOr<T>> errorOrTask,
            Func<T, IResult> onValue
        )
        {
            var errorOr = await errorOrTask;
            return errorOr.ToResult(onValue);
        }

        /// <summary>
        ///  Converts a list of errors into an <see cref="IResult"/> ProblemDetails response.
        /// </summary>
        public static IResult ToProblemResult(this List<Error> errors)
        {
            if (errors.Count is 0)
                return Results.Problem();

            if (errors.All(e => e.Type == ErrorType.Validation))
            {
                var extensions = new Dictionary<string, object?>
                {
                    ["errors"] = errors.ToDictionary(e => e.Code, e => e.Description),
                };

                return Results.ValidationProblem(
                    errors.ToDictionary(e => e.Code, e => new[] { e.Description }),
                    statusCode: StatusCodes.Status422UnprocessableEntity
                );
            }

            var first = errors[0];

            return Results.Problem(
                statusCode: first.ToHttpStatusCode(),
                title: first.Code,
                detail: first.Description
            );
        }

        // ─── Error → HTTP status code mapping ────────────────────────────────────

        /// <summary>
        ///  Maps an <see cref="ErrorType"/> to the canonical HTTP status code.
        /// </summary>
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

        // ─── ErrorOr<T> → IEnumerable convenience ────────────────────────────────

        /// <summary>
        ///  Returns all error descriptions as strings, or an empty enumerable when success.
        /// </summary>
        public static IEnumerable<string> ToErrorMessages<T>(this ErrorOr<T> errorOr) =>
            errorOr.IsError
                ? errorOr.Errors.Select(e => e.Description)
                : Enumerable.Empty<string>();

        /// <summary>
        ///  Returns all error codes as strings, or an empty enumerable when success.
        /// </summary>
        public static IEnumerable<string> ToErrorCodes<T>(this ErrorOr<T> errorOr) =>
            errorOr.IsError ? errorOr.Errors.Select(e => e.Code) : Enumerable.Empty<string>();

        // ─── Combine multiple ErrorOr results ────────────────────────────────────

        /// <summary>
        ///  Aggregates multiple <see cref="ErrorOr{T}"/> results: returns all errors
        ///  collected from every failed result, or the list of values if all succeeded.
        /// </summary>
        public static ErrorOr<IReadOnlyList<T>> Combine<T>(params ErrorOr<T>[] results)
        {
            var errors = results.Where(r => r.IsError).SelectMany(r => r.Errors).ToList();

            if (errors.Count > 0)
                return errors;

            IReadOnlyList<T> values = results.Select(r => r.Value).ToList().AsReadOnly();
            return ErrorOrFactory.From(values);
        }

        // ─── Null-guard helper ────────────────────────────────────────────────────

        /// <summary>
        ///  Converts a nullable value to an <see cref="ErrorOr{T}"/>.
        ///  Returns <paramref name="error"/> when the value is null.
        /// </summary>
        public static ErrorOr<T> ToErrorOrNotNull<T>(this T? value, Error error)
            where T : class => value is null ? error : value;

        public static ErrorOr<T> ToErrorOrNotNull<T>(this T? value, Error error)
            where T : struct => value is null ? error : value.Value;
    }
}
