namespace Expense_Tracker.Domain.Common.ResultPattern.Result;

using Expense_Tracker.Domain.Common.ResultPattern.Error;

public static class ResultExtensions
{
  
    /// <summary>
    /// Pattern matching for Result{T}.
    /// </summary>
    public static TResult Match<T, TResult>(
        this Result<T> result,
        Func<T, TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        if (result is SuccessResult<T> s)
            return onSuccess(s.Value);

        if (result is FailureResult<T> f)
            return onFailure(f.Error);

        throw new InvalidOperationException("Unhandled Result type.");
    }

    /// <summary>
    /// Pattern matching for non-generic Result.
    /// </summary>
    public static TResult Match<TResult>(
        this Result result,
        Func<TResult> onSuccess,
        Func<Error, TResult> onFailure)
    {
        if (result is SuccessResult)
            return onSuccess();

        if (result is FailureResult f)
            return onFailure(f.Error);

        throw new InvalidOperationException("Unhandled Result type.");
    }


    

    /// <summary>
    /// Transforms a success value while keeping errors unchanged.
    /// </summary>
    public static Result<U> Map<T, U>(
        this Result<T> result,
        Func<T, U> transform)
    {
        if (result is SuccessResult<T> s)
            return Result.Success(transform(s.Value));

        if (result is FailureResult<T> f)
            return Result.Failure<U>(f.Error);

        throw new InvalidOperationException("Unhandled Result type.");
    }



    public static Result Map(
        this Result result,
        Func<Result> transform)
    {
        if (result.IsSuccess)
            return transform();

        return Result.Failure(result.TryGetError());
    }


    // -----------------------------------------------------------
    // BIND (aka FlatMap / SelectMany)
    // -----------------------------------------------------------

    /// <summary>
    /// Chains another Result-producing function.
    /// </summary>
    public static Result<U> Bind<T, U>(
        this Result<T> result,
        Func<T, Result<U>> next)
    {
        if (result is SuccessResult<T> s)
            return next(s.Value);

        return Result.Failure<U>(result.TryGetError());
    }

    // -----------------------------------------------------------
    // ENSURE - Validate inside pipeline
    // -----------------------------------------------------------

    public static Result<T> Ensure<T>(
        this Result<T> result,
        Func<T, bool> predicate,
        Error error)
    {
        if (result is FailureResult<T>)
            return result;

        var value = result.TryGetValue();

        return predicate(value)
            ? result
            : Result.Failure<T>(error);
    }
}
