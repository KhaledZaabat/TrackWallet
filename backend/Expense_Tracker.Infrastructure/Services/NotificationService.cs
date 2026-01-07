using Expense_Tracker.Application.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Services;

public sealed class NotificationService(
    IEmailSender emailSender,
    IAppDbContext db,
    IUserContext userContext) : INotificationService
{
    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {
        if (!userContext.UserId.HasValue)
        {
            BackgroundJob.Enqueue<NotificationService>(x =>
                x.SendEmailNowAsync(to, subject, htmlBody));
            return;
        }

        bool shouldSend = await db.Users
            .Where(u => u.Id == userContext.UserId.Value)
            .Select(u => u.NotificationPreferences!.EmailNotifications)
            .FirstOrDefaultAsync(cancellationToken);

        if (!shouldSend)
        {
            return;
        }


        BackgroundJob.Enqueue<NotificationService>(x =>
            x.SendEmailNowAsync(to, subject, htmlBody));
    }

    public async Task SendEmailNowAsync(string to, string subject, string htmlBody)
    {
        await emailSender.SendEmailAsync(to, subject, htmlBody);
    }
}