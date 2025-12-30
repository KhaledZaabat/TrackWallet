using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Expense_Tracker.Application.EventHandlers.ResnedConfirmation;

public sealed class ResendConfirmationEventHandler(
    IOtpService _otpService,
    IEmailTemplateLoader _templateLoader,
    IEmailBodyBuilder _bodyBuilder,
    INotificationService _notification,
    OtpSettings _otpSettings
) : INotificationHandler<ResendConfirmationEvent>
{
    public async Task Handle(ResendConfirmationEvent notification, CancellationToken cancellationToken)
    {
        var user = notification.User;
        if (user is null)
            return;

        string email = user.Email.ToLowerInvariant().Trim();
        if (string.IsNullOrWhiteSpace(email))
            return;

        string userName = user.UserName.Trim();
        await SendResendConfirmationEmailAsync(email, userName, cancellationToken);
    }

    private async Task SendResendConfirmationEmailAsync(
        string email,
        string userName,
        CancellationToken cancellationToken)
    {
        // Generate NEW OTP (this invalidates any previous codes)
        string key = $"confirm:{email.ToLowerInvariant().Trim()}";
        string otp = _otpService.Generate(key, digits: _otpSettings.Digits);

        // Load resend confirmation template
        string template = await _templateLoader.LoadTemplateAsync(
            EmailTemplates.ResendConfirmationTemplate,
            cancellationToken);

        // Replace placeholders
        var body = _bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Email"] = email,
            ["OTP"] = otp,
            ["Duration"] = _otpSettings.ExpirationInSeconds.ToString()
        });

        // Send resend confirmation email
        await _notification.SendEmailAsync(
            to: email,
            subject: "Your New Verification Code - Expense Tracker",
            htmBody: body,
            cancellationToken: cancellationToken
        );
    }
}