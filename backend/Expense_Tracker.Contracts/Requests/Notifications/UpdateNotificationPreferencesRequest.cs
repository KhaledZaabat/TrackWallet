namespace Expense_Tracker.Contracts.Requests.Notifications;




public sealed record UpdateNotificationPreferencesRequest(
    bool EmailNotifications,
    bool PushNotifications);
