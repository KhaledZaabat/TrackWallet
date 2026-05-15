using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;

namespace Expense_Tracker.Application.EventHandlers.User;

/// <summary>
/// Sends the welcome email when a new account is registered. The email carries
/// a magic link
/// (<c>https://app/auth/confirm?email=…&amp;token=…</c>) where the token is
/// produced by ASP.NET Identity's
/// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}.GenerateEmailConfirmationTokenAsync"/>.
/// The token is HMAC-protected, bound to the user's id + SecurityStamp, and
/// requires no server-side storage.
/// </summary>
public sealed class UserCreatedEventHandler(
    IIdentityService identityService,
    IEmailLinkService emailLinks,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
{
    private const int TokenLifespanMinutes = 15;

    public async Task Handle(UserCreatedEvent evt, CancellationToken ct)
    {
        var user = evt.User;
        if (user is null) return;

        string email = user.Email?.ToLowerInvariant().Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(email)) return;

        await SendWelcomeEmailAsync(email, user.UserName.Trim(), ct);
    }

    private async Task SendWelcomeEmailAsync(string email, string userName, CancellationToken ct)
    {
        var tokenResult = await identityService.GenerateEmailConfirmationTokenAsync(email);
        if (tokenResult.IsError) return; // user vanished between save & event — best-effort.

        string link = emailLinks.BuildConfirmEmailLink(email, tokenResult.Value);

        string template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.UserCreatedTemplate, ct);

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
            subject: "Welcome to Expense Tracker — confirm your email",
            htmBody: body,
            cancellationToken: ct);
    }
}
