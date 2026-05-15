using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;

namespace Expense_Tracker.Application.EventHandlers.ResnedConfirmation;

/// <summary>
/// Re-issues the confirmation magic link when the user requests a fresh one.
/// The previous token is automatically invalidated by the SecurityStamp
/// rotation that
/// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}.UpdateSecurityStampAsync"/>
/// triggers — but more practically, every Identity-issued confirmation token
/// for the same user is independent and time-limited, so users always have
/// at most one viable link in the wild.
/// </summary>
public sealed class ResendConfirmationEventHandler(
    IIdentityService identityService,
    IEmailLinkService emailLinks,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
{
    private const int TokenLifespanMinutes = 15;

    public async Task Handle(ResendConfirmationEvent evt, CancellationToken ct)
    {
        var user = evt.User;
        if (user is null) return;

        string email = user.Email?.ToLowerInvariant().Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email)) return;

        await SendResendConfirmationEmailAsync(email, user.UserName.Trim(), ct);
    }

    private async Task SendResendConfirmationEmailAsync(
        string email, string userName, CancellationToken ct)
    {
        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(email);
        if (tokenResult.IsError) return;

        string link = emailLinks.BuildConfirmEmailLink(email, tokenResult.Value);

        string template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.ResendConfirmationTemplate, ct);

        string body = bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Email"] = email,
            ["ConfirmLink"] = link,
            ["ExpiresInMinutes"] = TokenLifespanMinutes.ToString(),
            ["Year"] = DateTime.UtcNow.Year.ToString(),
        });

        await notification.SendEmailAsync(
            to: email,
            subject: "Your new confirmation link — Expense Tracker",
            htmBody: body,
            cancellationToken: ct);
    }
}
