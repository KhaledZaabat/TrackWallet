namespace Expense_Tracker.Domain.Common;

public interface ICreatable
{
    DateTimeOffset CreatedAtUtc { get; set; }
    Guid CreatedBy { get; set; }
}
