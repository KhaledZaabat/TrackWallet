using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Events;
using MediatR;

namespace Expense_Tracker.Application.EventHandlers.User;

public sealed class UserCreatedEventHandler(IOtpService _otpService,
    IEmailTemplateLoader _templateLoader,
    IEmailBodyBuilder _bodyBuilder,
    INotificationService _notification,
    OtpSettings _otpSettings
    )
    : INotificationHandler<UserCreatedEvent>
{




    public async Task Handle(UserCreatedEvent notification, CancellationToken cancellationToken)
    {
        var user = notification.User;
        if (user is null)
            return;

        string email = user.Email.ToLowerInvariant().Trim() ?? string.Empty;
        if (email is null) return;
        string userName = notification.User.UserName.Trim();


        await SendOtpEmailAsync(email, userName, cancellationToken);

    }

    private async Task SendOtpEmailAsync(
        string email,
        string userName,
        CancellationToken cancellationToken)
    {
        // Generate OTP using settings
        string key = $"confirm:{email.ToLowerInvariant().Trim()}";
        string otp = _otpService.Generate(key, digits: _otpSettings.Digits);

        // Load template
        string template = await _templateLoader.LoadTemplateAsync(, cancellationToken);

        // Replace placeholders
        var body = _bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Email"] = email,
            ["OTP"] = otp,
            ["Duration"] = _otpSettings.ExpirationInSeconds.ToString()
        });

        // Send email
        await _notification.SendEmailAsync(
            to: email,
            subject: "Your Expense Tracker Verification Code",
            htmBody: body,
            cancellationToken: cancellationToken
        );
    }
}