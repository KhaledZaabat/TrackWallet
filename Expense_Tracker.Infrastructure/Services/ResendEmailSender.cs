using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Expense_Tracker.Infrastructure.Email;
using Resend;

namespace Expense_Tracker.Infrastructure.Services;

public sealed class ResendEmailSender : IEmailSender
{
    private readonly ResendSettings _settings;
    private readonly ILogger<ResendEmailSender> _logger;
    private readonly IResend _resend;

    public ResendEmailSender(
        IOptions<ResendSettings> settings,
        ILogger<ResendEmailSender> logger,
        IResend resend)
    {
        _settings = settings.Value;
        _logger = logger;
        _resend = resend;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new EmailMessage
        {
            From = _settings.From,
            Subject = subject,
            HtmlBody = htmlMessage
        };

        message.To.Add(email);

        try
        {
            var result = await _resend.EmailSendAsync(message);
            _logger.LogInformation("Email sent via Resend to {Recipient}. Id={Id}", email, result.Content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email via Resend");
            throw;
        }
    }
}