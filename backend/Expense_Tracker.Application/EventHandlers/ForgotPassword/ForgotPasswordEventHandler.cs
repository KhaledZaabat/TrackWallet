using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using MediatR;

namespace Expense_Tracker.Application.EventHandlers.ForgotPassword;

public sealed class ForgotPasswordEventHandler(IOtpService _otpService, IEmailTemplateLoader _templateLoader, IEmailBodyBuilder _bodyBuilder, INotificationService _notification, OtpSettings otpSettings)
    : INotificationHandler<ForgotPasswordEvent>
{






    public async Task Handle(ForgotPasswordEvent notification, CancellationToken cancellationToken)
    {

        string email = notification.Email ?? string.Empty;
        string fullName = notification.UserName ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email))
            return;
        await SendOtpEmailAsync(email, fullName, cancellationToken);

    }

    private async Task SendOtpEmailAsync(
        string email,
        string userName,
        CancellationToken cancellationToken)
    {
        // Generate OTP
        string key = $"reset:{email}";
        string otp = _otpService.Generate(key, digits: 4);

        // Load email template
        string template = await _templateLoader.LoadTemplateAsync(
            EmailTemplates.ForgotPasswordOtp,
            cancellationToken);

        // Build HTML body
        string body = _bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Email"] = email,
            ["OTP"] = otp,
            ["Duration"] = otpSettings.ExpirationInSeconds.ToString()
        });

        // Send email
        await _notification.SendEmailAsync(
            to: email,
            subject: "Reset Your Expense Tracker Account Password",
            htmBody: body,
            cancellationToken: cancellationToken
        );
    }
}
