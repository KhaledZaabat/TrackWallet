using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Application.Events;

namespace Expense_Tracker.Application.EventHandlers.User;

public sealed class UserCreatedEventHandler(
    IOtpService _otpService,
    IEmailTemplateLoader _templateLoader,
    IEmailBodyBuilder _bodyBuilder,
    INotificationService _notification,
    OtpSettings _otpSettings
)
{
    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var user = notification.User;
        if (user is null)
            return;

        string email = user.Email.ToLowerInvariant().Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email))
            return;

        string userName = notification.User.UserName.Trim();
        await SendWelcomeEmailAsync(email, userName, cancellationToken);
    }

    private async Task SendWelcomeEmailAsync(
        string email,
        string userName,
        CancellationToken cancellationToken)
    {
        string key = $"confirm:{email.ToLowerInvariant().Trim()}";
        string otp = _otpService.Generate(key, digits: _otpSettings.Digits);

        string template = await _templateLoader.LoadTemplateAsync(
            EmailTemplates.UserCreatedTemplate,
            cancellationToken);

        var body = _bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Email"] = email,
            ["OTP"] = otp,
            ["Duration"] = _otpSettings.ExpirationInSeconds.ToString()
        });

        await _notification.SendEmailAsync(
            to: email,
            subject: "Welcome to Expense Tracker - Verify Your Email",
            htmBody: body,
            cancellationToken: cancellationToken
        );
    }
}