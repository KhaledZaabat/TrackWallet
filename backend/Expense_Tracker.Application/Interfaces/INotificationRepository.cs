using ErrorOr;

namespace Expense_Tracker.Application.Interfaces;

public interface INotificationRepository
{
    Task<ErrorOr<Success>> MarkAsReadAsync(
        Guid notificationId,
        Guid userId,
        CancellationToken cancellationToken);

    Task AddAsync(
        DomainNotification notification,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<DomainNotification>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
