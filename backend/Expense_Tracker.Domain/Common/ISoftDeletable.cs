using ErrorOr;

namespace Expense_Tracker.Domain.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
    Guid? DeletedById { get; set; }
    DateTimeOffset? DeletedOn { get; set; }

    ErrorOr<Success> SoftDelete(Guid deletedBy);
}
