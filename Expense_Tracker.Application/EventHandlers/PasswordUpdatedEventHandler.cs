using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using MediatR;

namespace Expense_Tracker.Application.Common.Events.UserCreated;

public sealed class PasswordUpdatedEventHandler
    : INotificationHandler<PasswordUpdatedEvent>
{
    private readonly IEmailTemplateLoader _templateLoader;
    private readonly IEmailBodyBuilder _bodyBuilder;
    private readonly INotificationService _notification;

    public PasswordUpdatedEventHandler(
        IEmailTemplateLoader templateLoader,
        IEmailBodyBuilder bodyBuilder,
        INotificationService notification)
    {
        _templateLoader = templateLoader;
        _bodyBuilder = bodyBuilder;
        _notification = notification;
    }

    public async Task Handle(
        PasswordUpdatedEvent e,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(e.Email))
            return;

        string template = await _templateLoader.LoadTemplateAsync(
            EmailTemplates.PasswordUpdated,
            cancellationToken);

        string body = _bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["UserName"] = e.FullName,
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
