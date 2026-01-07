using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Events;
using Expense_Tracker.Domain.PushNotifications.Enums;
using Expense_Tracker.Domain.TransactionFolder.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.EventHandlers.Inv;

public sealed class TransactionCreatedEventHandler(
    IAppDbContext context,
    IFcmTopicService fcmTopicService)
    : INotificationHandler<TransactionCreatedEvent>
{
    public async Task Handle(
        TransactionCreatedEvent notification,
        CancellationToken ct)
    {
        var transaction = notification.transaction;

        // Fetch creator name
        var creatorName = await context.Users
            .Where(u => u.Id == transaction.CreatedById)
            .Select(u => u.UserName)
            .SingleOrDefaultAsync(ct);

        // Fetch family name
        var familyName = await context.Families
            .Where(f => f.Id == transaction.FamilyId)
            .Select(f => f.Name)
            .SingleOrDefaultAsync(ct);

        var title = transaction.Type == TransactionType.Expense
            ? "💸 New expense added"
            : "💰 New income added";

        var body =
            $"{creatorName} added {transaction.Amount} in {familyName} Family";

        var domainNotification = DomainNotification.Create(
            userId: transaction.CreatedById, // topic-based
            title: title,
            body: body,
            type: NotificationType.TransactionCreated,
            actorUserId: transaction.CreatedById,
            data: new Dictionary<string, string>
            {
                ["transactionId"] = transaction.Id.ToString(),
                ["familyId"] = transaction.FamilyId.ToString(),
                ["categoryId"] = transaction.CategoryId.ToString(),
                ["amount"] = transaction.Amount.ToString(),
                ["transactionType"] = transaction.Type.ToString(),
                ["action"] = "open-transaction"
            });

        var familyTopic = Topics.getFamilyTopic(transaction.FamilyId);

        await fcmTopicService.SendToTopicAsync(
            familyTopic,
            domainNotification,
            ct);
    }
}