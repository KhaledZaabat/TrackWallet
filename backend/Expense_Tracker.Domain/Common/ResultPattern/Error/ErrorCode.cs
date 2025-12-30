namespace Expense_Tracker.Domain.Common.ResultPattern.Error;

public abstract record ErrorCode(string Value)
{
    public override string ToString() => Value;
}