using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Users;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandHandler(
    IAppDbContext db,
    IUserDeviceRepository deviceRepository)
    : IRequestHandler<UpdateNotificationPreferencesCommand, Result>
{
    public async Task<Result> Handle(
        UpdateNotificationPreferencesCommand request,
        CancellationToken cancellationToken)
    {
        // Get user with preferences
        User? user = await db.Users
            .Include(u => u.NotificationPreferences)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(DomainError.NotFound(nameof(User)));

        if (request.PushNotifications is false && request.EmailNotifications is false)
        {
            return Result.Failure(
                NotificationPreferencesError.InvalidState(
                    "At least one of preferences must be enabled"));
        }

        // Store the old push notification state to detect changes
        bool wasPushEnabled = user.NotificationPreferences?.PushNotifications ?? false;

        // Get or create notification preferences
        NotificationPreferences? preferences = await db.NotificationPreferences
            .FirstOrDefaultAsync(
                np => np.PushNotifications == request.PushNotifications
                   && np.EmailNotifications == request.EmailNotifications,
                cancellationToken);

        if (preferences is null)
            return Result.Failure(
                NotificationPreferencesError.InvalidState(
                    "Preferences not found"));

        // Update user's notification preferences
        user.UpdateNotificationPreferences(preferences.Id);

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

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private async Task DeactivateUserDevicesAsync(
           Guid userId,
           CancellationToken cancellationToken)
    {
        await db.UserDevices
            .Where(d => d.UserId == userId && d.IsActive)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(d => d.IsActive, false)
                    .SetProperty(d => d.LastSeenUtc, DateTime.UtcNow),
                cancellationToken);
    }

    private async Task ActivateUserDevicesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        await db.UserDevices
            .Where(d => d.UserId == userId && !d.IsActive)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(d => d.IsActive, true)
                    .SetProperty(d => d.LastSeenUtc, DateTime.UtcNow),
                cancellationToken);
    }
}