namespace Expense_Tracker.Contracts.Reponses.Notifications;

public sealed record NotificationListResponse(
    IReadOnlyList<NotificationItem> Items,
    int UnreadCount,
    int Skip,
    int Take);

public sealed record UnreadCountResponse(int UnreadCount);

public sealed record MarkAllAsReadResponse(int Updated);
