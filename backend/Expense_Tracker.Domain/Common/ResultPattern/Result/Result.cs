namespace Expense_Tracker.Domain.Common.ResultPattern.Result;

using Expense_Tracker.Domain.Common.ResultPattern.Error;

public abstract record Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;

    protected Result(bool isSuccess)
    {
        IsSuccess = isSuccess;
    }

    public Error TryGetError()
    {
        if (this is FailureResult failure)
            return failure.Error;

        throw new InvalidOperationException("Cannot access Error of a success result.");
    }


    public static Result Success() => new SuccessResult();

    public static Result Failure(Error error) =>
        new FailureResult(error);

    public static Result<T> Success<T>(T value) =>
        new SuccessResult<T>(value);

    public static Result<T> Failure<T>(Error error) =>
        new FailureResult<T>(error);
}





public abstract record Result<T> : Result
{
    protected Result(bool isSuccess)
        : base(isSuccess)
    {
    }

    public Error TryGetError()
    {
        if (this is FailureResult<T> failure)
            return failure.Error;

        throw new InvalidOperationException("Cannot access Error of a success result.");
    }

    public T TryGetValue()
    {
        if (this is SuccessResult<T> success)
            return success.Value;

        throw new InvalidOperationException("Cannot access Value of a failed result.");
    }
}
