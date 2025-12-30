using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using MediatR;

namespace Expense_Tracker.Application.EventHandlers.User;

public sealed class PasswordUpdatedEventHandler(IEmailTemplateLoader _templateLoader, IEmailBodyBuilder _bodyBuilder, INotificationService _notification)
    : INotificationHandler<PasswordUpdatedEvent>
{

    public async Task Handle(
        PasswordUpdatedEvent e,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(e.Email))
            return;

        string template = await _templateLoader.LoadTemplateAsync(
            EmailTemplates.PasswordUpdatedTemplate,
            cancellationToken);

        string body = _bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["UserName"] = e.UserName,
            ["Email"] = e.Email,
            ["Date"] = e.Timestamp.ToString("yyyy-MM-dd HH:mm:ss 'UTC'"),
            ["IPAddress"] = e.IpAddress,
            ["Year"] = DateTime.UtcNow.Year.ToString()
        });

        await _notification.SendEmailAsync(
            to: e.Email,
            subject: "Your Password Has Been Updated",
            htmBody: body,
            cancellationToken: cancellationToken
        );
    }
}
