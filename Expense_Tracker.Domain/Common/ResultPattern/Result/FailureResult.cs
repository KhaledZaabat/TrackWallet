namespace Expense_Tracker.Domain.Common.ResultPattern.Result;

using Expense_Tracker.Domain.Common.ResultPattern.Error;

public sealed record FailureResult(Error Error) : Result(false);
public sealed record FailureResult<T>(Error Error)
    : Result<T>(false);