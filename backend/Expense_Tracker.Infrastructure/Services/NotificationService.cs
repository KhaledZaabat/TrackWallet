using Expense_Tracker.Application.Interfaces;
using Hangfire;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace Expense_Tracker.Infrastructure.Services;

public sealed class NotificationService(
    IEmailSender emailSender
    ) : INotificationService
{
    public async Task SendEmailAsync(
        string to,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken = default)
    {


        BackgroundJob.Enqueue<NotificationService>(x =>
            x.SendEmailNowAsync(to, subject, htmlBody));
    }

    public async Task SendEmailNowAsync(string to, string subject, string htmlBody)
    {
        await emailSender.SendEmailAsync(to, subject, htmlBody);
    }
}