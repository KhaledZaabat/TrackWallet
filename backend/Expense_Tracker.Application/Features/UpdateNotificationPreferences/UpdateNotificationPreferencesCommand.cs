namespace Expense_Tracker.Application.Features.UpdateNotificationPreferences;

public sealed record UpdateNotificationPreferencesCommand(
    Guid UserId,
    bool EmailNotifications,
    bool PushNotifications
);
