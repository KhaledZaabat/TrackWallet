namespace Expense_Tracker.Domain.Common;

public interface IUpdatable
{
    DateTimeOffset LastModifiedUtc { get; set; }
    Guid LastModifiedBy { get; set; }
}
