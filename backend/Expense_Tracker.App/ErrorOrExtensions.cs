using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Expense_Tracker.App
{
    public static class ErrorOrExtensions
    {
        public static IActionResult ToActionResult<T>(
            this ErrorOr<T> errorOr,
            Func<T, IActionResult> onValue,
            ControllerBase controller
        )
        {
            return errorOr.Match(value => onValue(value), errors => controller.Problem(errors));
        }
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
        public static async Task<IActionResult> ToActionResultAsync<T>(
            this Task<ErrorOr<T>> errorOrTask,
            Func<T, IActionResult> onValue,
            ControllerBase controller
        )
        {
            var errorOr = await errorOrTask;
            return errorOr.ToActionResult(onValue, controller);
        }
        public static async Task<ActionResult<T>> ToActionResultAsync<T>(
            this Task<ErrorOr<T>> errorOrTask,
            ControllerBase controller
        )
        {
            var errorOr = await errorOrTask;
            return errorOr.ToActionResult(controller);
        }
        public static async Task<IActionResult> ToActionResultAsync(
            this Task<ErrorOr<Success>> errorOrTask,
            ControllerBase controller
        )
        {
            var errorOr = await errorOrTask;
            return errorOr.ToActionResult(controller);
        }
        public static ActionResult Problem(this ControllerBase controller, List<Error> errors)
        {
            if (errors.Count is 0)
                return new ObjectResult(
                    controller.ProblemDetailsFactory.CreateProblemDetails(controller.HttpContext)
                )
                {
                    StatusCode = StatusCodes.Status500InternalServerError,
                };

            if (errors.All(e => e.Type == ErrorType.Validation))
            {
                var modelState = new ModelStateDictionary();
                foreach (var error in errors)
                    modelState.AddModelError(error.Code, error.Description);

                return new UnprocessableEntityObjectResult(
                    new ValidationProblemDetails(modelState)
                );
            }

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
        public static IResult ToResult<T>(this ErrorOr<T> errorOr, Func<T, IResult> onValue)
        {
            return errorOr.Match(value => onValue(value), errors => errors.ToProblemResult());
        }
        public static async Task<IResult> ToResultAsync<T>(
            this Task<ErrorOr<T>> errorOrTask,
            Func<T, IResult> onValue
        )
        {
            var errorOr = await errorOrTask;
            return errorOr.ToResult(onValue);
        }
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
            errorOr.IsError
                ? errorOr.Errors.Select(e => e.Description)
                : Enumerable.Empty<string>();
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
    }
}
