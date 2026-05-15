using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Notifications;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class TransactionCreatedEventHandler(
    IRepository<global::Expense_Tracker.Domain.Users.User> users,
    IRepository<Family> families,
    IFcmTopicService fcmTopicService,
    INotificationBuilder notificationBuilder)
{
    public async Task Handle(TransactionCreatedEvent e, CancellationToken ct)
    {
        var transaction = e.Transaction;

        string? creatorName = await users.Query()
            .Where(u => u.Id == transaction.CreatedById)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        string? familyName = await families.Query()
            .Where(f => f.Id == transaction.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        DomainNotification domainNotification = notificationBuilder.Build(
            recipientUserId: transaction.CreatedById,
            actorUserId: transaction.CreatedById,
            payload: new TransactionCreatedPayload(
                TransactionId: transaction.Id,
                FamilyId: transaction.FamilyId,
                FamilyName: familyName ?? string.Empty,
                CategoryId: transaction.CategoryId,
                Amount: transaction.Amount,
                TransactionType: transaction.Type.ToString(),
                CreatorUserId: transaction.CreatedById,
                CreatorUserName: creatorName ?? string.Empty));

        string familyTopic = Topics.getFamilyTopic(transaction.FamilyId);
        await fcmTopicService.SendToTopicAsync(familyTopic, domainNotification, ct);
    }
}
