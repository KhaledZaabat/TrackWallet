using ErrorOr;
using Expense_Tracker.Domain.PushNotifications;

namespace Expense_Tracker.Application.Interfaces;

public interface INotificationRepository
{
    /// <summary>
    /// Page through a user's notifications newest-first.
    /// </summary>
    /// <param name="userId">Owner.</param>
    /// <param name="onlyUnread">When true, restricts to <c>IsRead == false</c>.</param>
    /// <param name="skip">Items to skip (page offset). Must be &gt;= 0.</param>
    /// <param name="take">Items to return (page size). Capped server-side.</param>
    Task<IReadOnlyList<DomainNotification>> ListAsync(
        Guid userId,
        bool onlyUnread,
        int skip,
        int take,
        CancellationToken cancellationToken);

    Task<int> CountUnreadAsync(Guid userId, CancellationToken cancellationToken);

    Task AddAsync(DomainNotification notification, CancellationToken cancellationToken);

    /// <summary>
    /// Marks a single notification as read. Idempotent — calling twice is a no-op.
    /// Persists immediately so callers do not need to remember to flush.
    /// </summary>
    Task<ErrorOr<Success>> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken);

    /// <summary>
    /// Marks every unread notification owned by <paramref name="userId"/> as read
    /// in a single round-trip. Returns the number of rows affected.
    /// </summary>
    Task<int> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken);
}
