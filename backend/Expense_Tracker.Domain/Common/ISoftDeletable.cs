using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    Guid? DeletedById { get; set; }
    DateTimeOffset? DeletedOn { get; set; }

    public Result SoftDelete(Guid deletedBy);

}