using Microsoft.AspNetCore.Mvc;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.App.Helpers;







//public static class ResultExtensions
//{

//    public static ActionResult<T> ToActionResult<T>(this Result<T> result, HttpContext? context = null)
//    {
//        return result switch
//        {
//            SuccessResult<T> success => new OkObjectResult(success.Value),

//            FailureResult<T> failure => failure.ToProblem(context),

//            _ => new ObjectResult(new { message = "Unknown error" })
//            {
//                StatusCode = StatusCodes.Status500InternalServerError
//            }
//        };
//    }


//    public static IActionResult ToActionResult(this Result result, HttpContext? context = null)
//    {
//        return result switch
//        {
//            SuccessResult => new OkObjectResult(new { message = "Operation successful" }),
//            FailureResult failure => failure.ToProblem(context),
//            _ => new ObjectResult(new { message = "Unknown error" })
//            {
//                StatusCode = StatusCodes.Status500InternalServerError
//            }
//        };
//    }

//    public static ActionResult ToProblem<T>(this Result<T> result, HttpContext? context = null)
//    {
//        var error = result.Error;
//        int statusCode = ErrorMapper.ToHttpStatus(error.Code);


//        var problem = Results.Problem(statusCode: statusCode);

//        ProblemDetails problemDetails = (problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails)!;
//        problemDetails.Extensions["Errors"] = new
//        {
//            title = result.Error.Code,
//            Description = result.Error.Description,
//        };
//        problemDetails.Extensions["Instance"] = context != null ? $"{context.Request.Method} {context.Request.Path}" : null;
//        return new ObjectResult(problemDetails)
//        {
//            StatusCode = statusCode,
//            ContentTypes = { "application/problem+json" }
//        };




//    }


//    public static IActionResult ToProblem(this Result result, HttpContext? contex = null)
//    {
//        var error = result.Error;
//        int statusCode = ErrorMapper.ToHttpStatus(error.Code);

//        var problem = Results.Problem(statusCode: statusCode);

//        ProblemDetails problemDetails = (problem.GetType().GetProperty(nameof(ProblemDetails))!.GetValue(problem) as ProblemDetails)!;
//        problemDetails.Extensions["Errors"] = new
//        {
//            title = result.Error.Code,
//            Description = result.Error.Description,
//        };
//        problemDetails.Extensions["Instance"] = context != null ? $"{context.Request.Method} {context.Request.Path}" : null;

//        return new ObjectResult(problemDetails)
//        {
//            StatusCode = statusCode,
//            ContentTypes = { "application/problem+json" }
//        };


//    }
//}


public static class ResultExtensions
{
    public static ActionResult<T> ToActionResult<T>(this Result<T> result, HttpContext? context = null)
    {
        return result switch
        {
            SuccessResult<T> success => new OkObjectResult(success.Value),
            FailureResult<T> failure => failure.ToProblem(context),
            _ => new ObjectResult(new { message = "Unknown error" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
    }

    public static IActionResult ToActionResult(this Result result, HttpContext? context = null)
    {
        return result switch
        {
            SuccessResult => new OkObjectResult(new { message = "Operation successful" }),
            FailureResult failure => failure.ToProblem(context),
            _ => new ObjectResult(new { message = "Unknown error" })
            {
                StatusCode = StatusCodes.Status500InternalServerError
            }
        };
    }

    private static ActionResult ToProblem<T>(this FailureResult<T> result, HttpContext? context = null)
    {
        Error error = result.Error;
        int statusCode = error.ToHttpStatus();

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type,
            Detail = error.Description,
            Instance = context != null ? $"{context.Request.Method} {context.Request.Path}" : null
        };

        //  structured extensions
        problemDetails.Extensions["ErrorCode"] = error.Code.ToString();
        problemDetails.Extensions["ErrorType"] = error.Type;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }

    private static IActionResult ToProblem(this FailureResult result, HttpContext? context = null)
    {
        Error error = result.Error;
        int statusCode = error.ToHttpStatus();

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Type,
            Detail = error.Description,
            Instance = context != null ? $"{context.Request.Method} {context.Request.Path}" : null
        };

        problemDetails.Extensions["ErrorCode"] = error.Code.ToString();
        problemDetails.Extensions["ErrorType"] = error.Type;

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = { "application/problem+json" }
        };
    }
}