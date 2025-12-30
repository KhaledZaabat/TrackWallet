namespace Expense_Tracker.Application.Interfaces;

public interface IUnifiedNotificationDispatcher
{
    Task EnqueueAsync(
        DomainNotification notification,
        CancellationToken ct);
}