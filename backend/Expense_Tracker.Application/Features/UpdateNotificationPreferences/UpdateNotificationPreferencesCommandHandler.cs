using Expense_Tracker.Domain.Users;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using ErrorOr;
using Expense_Tracker.Domain.Errors;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandHandler(
    IRepository<Expense_Tracker.Domain.Users.User> users,
    IRepository<NotificationPreferences> notificationPreferences,
    IRepository<Expense_Tracker.Domain.PushNotifications.UserDevice> userDevices)
{
    public async Task<ErrorOr<Success>> Handle(
        UpdateNotificationPreferencesCommand request,
        CancellationToken cancellationToken)
    {
        // Get user with preferences
        Expense_Tracker.Domain.Users.User? user = await users.QueryTracked()
            .Include(u => u.NotificationPreferences)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return DomainErrors.GeneralErrors.NotFound(nameof(Expense_Tracker.Domain.Users.User));

        if (request.PushNotifications is false && request.EmailNotifications is false)
        {
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(NotificationPreferences),
                "At least one of preferences must be enabled");
        }

        // Store the old push notification state to detect changes
        bool wasPushEnabled = user.NotificationPreferences?.PushNotifications ?? false;

        // Get or create notification preferences
        NotificationPreferences? preferences = await notificationPreferences.QueryTracked()
            .FirstOrDefaultAsync(
                np => np.PushNotifications == request.PushNotifications
                   && np.EmailNotifications == request.EmailNotifications,
                cancellationToken);

        if (preferences is null)
            return DomainErrors.GeneralErrors.InvalidState(
                nameof(NotificationPreferences),
                "Preferences not found");

        // Update user's notification preferences
        ErrorOr<Success> updateResult = user.UpdateNotificationPreferences(preferences.Id);
        if (updateResult.IsError)
            return updateResult.Errors;

        // Handle device activation/deactivation based on push notification state change
        if (wasPushEnabled && !preferences.PushNotifications)
        {
            // User disabled push notifications - deactivate all their devices
            await DeactivateUserDevicesAsync(request.UserId, cancellationToken);
        }
        else if (!wasPushEnabled && preferences.PushNotifications)
        {
            // User enabled push notifications - reactivate all their devices
            await ActivateUserDevicesAsync(request.UserId, cancellationToken);
        }

        await users.SaveChangesAsync(cancellationToken);
        return new Success();
    }

    private async Task DeactivateUserDevicesAsync(
           Guid userId,
           CancellationToken cancellationToken)
    {
        await userDevices.QueryTracked()
            .Where(d => d.UserId == userId && d.IsActive)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(d => d.IsActive, false)
                    .SetProperty(d => d.LastModifiedUtc, DateTimeOffset.UtcNow),
                cancellationToken);
    }

    private async Task ActivateUserDevicesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await userDevices.QueryTracked()
            .Where(d => d.UserId == userId && !d.IsActive)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(d => d.IsActive, true)
                    .SetProperty(d => d.LastModifiedUtc, DateTimeOffset.UtcNow),
                cancellationToken);
    }
}
