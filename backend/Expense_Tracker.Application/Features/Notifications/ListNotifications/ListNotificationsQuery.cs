using Expense_Tracker.Contracts.Reponses.Notifications;

namespace Expense_Tracker.Application.Features.Notifications.ListNotifications;

public sealed record ListNotificationsQuery(
    bool OnlyUnread,
    int Skip,
    int Take);
