using Expense_Tracker.Domain.PushNotifications.Enums;

namespace Expense_Tracker.Application.Interfaces;

public interface IUserDeviceRepository
{
    Task UpsertAsync(Guid userId, string token, PushPlatform platform, CancellationToken cancellationToken);
    Task UnbindDeviceAsync(string token, CancellationToken cancellationToken);
    Task ClearUserAsync(Guid userId, CancellationToken cancellationToken);
    Task<IReadOnlyList<string>> GetActiveTokensAsync(Guid userId, CancellationToken cancellationToken);
    Task RemoveTokenAsync(string token, CancellationToken cancellationToken);

    Task<IReadOnlyList<string>> GetUserDeviceTokensAsync(Guid userId, CancellationToken cancellationToken);
    Task SubscribeToTopicAsync(Guid userId, string topic, CancellationToken cancellationToken);
    Task UnsubscribeFromTopicAsync(Guid userId, string topic, CancellationToken cancellationToken);
}