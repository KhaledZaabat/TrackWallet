using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using Expense_Tracker.Infrastructure.Email;

public sealed class EmailSender : IEmailSender
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(
        IOptions<EmailSettings> settings,
        ILogger<EmailSender> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(email));
        message.Subject = subject;

        message.Body = new TextPart("html")
        {
            Text = htmlMessage
        };

        using var client = new SmtpClient();

        try
        {
            await client.ConnectAsync(
                _settings.Host,
                _settings.Port,
                _settings.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto);

            if (!string.IsNullOrWhiteSpace(_settings.Username))
            {
                await client.AuthenticateAsync(
                    _settings.Username,
                    _settings.Password);
            }

            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }

        _logger.LogInformation("Email sent to {Recipient}", email);
    }
}
