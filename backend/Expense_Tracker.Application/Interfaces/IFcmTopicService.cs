using Expense_Tracker.Domain.PushNotifications;

namespace Expense_Tracker.Application.Interfaces;

public interface IFcmTopicService
{
    Task SubscribeToTopicAsync(
        IEnumerable<string> deviceTokens,
        string topic,
        CancellationToken cancellationToken = default);

    Task UnsubscribeFromTopicAsync(
        IEnumerable<string> deviceTokens,
        string topic,
        CancellationToken cancellationToken = default);

    Task SendToTopicAsync(
        string topic,
        DomainNotification notification,
        CancellationToken cancellationToken = default);
}