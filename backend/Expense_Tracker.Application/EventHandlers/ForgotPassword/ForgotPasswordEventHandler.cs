using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;

namespace Expense_Tracker.Application.EventHandlers.ForgotPassword;

/// <summary>
/// Sends the password-reset magic link. The token comes from
/// <see cref="Microsoft.AspNetCore.Identity.UserManager{TUser}.GeneratePasswordResetTokenAsync"/>
/// — bound to the user's SecurityStamp, so a successful password change rotates
/// the stamp and immediately invalidates every other unused reset token in the
/// wild.
/// </summary>
public sealed class ForgotPasswordEventHandler(
    IIdentityService identityService,
    IEmailLinkService emailLinks,
    IEmailTemplateLoader templateLoader,
    IEmailBodyBuilder bodyBuilder,
    INotificationService notification)
{
    private const int TokenLifespanMinutes = 15;

    public async Task Handle(ForgotPasswordEvent evt, CancellationToken ct)
    {
        string email = evt.Email?.ToLowerInvariant().Trim() ?? string.Empty;
        string fullName = evt.UserName ?? string.Empty;

        if (string.IsNullOrWhiteSpace(email)) return;

        await SendResetEmailAsync(email, fullName, ct);
    }

    private async Task SendResetEmailAsync(string email, string userName, CancellationToken ct)
    {
        var tokenResult = await identityService.GeneratePasswordResetTokenAsync(email);
        if (tokenResult.IsError) return;

        string link = emailLinks.BuildResetPasswordLink(email, tokenResult.Value);

        string template = await templateLoader.LoadTemplateAsync(
            EmailTemplates.ResetPasswordTemplate, ct);

        string body = bodyBuilder.Build(template, new Dictionary<string, string>
        {
            ["UserName"] = userName,
            ["Email"] = email,
            ["ResetLink"] = link,
            ["ExpiresInMinutes"] = TokenLifespanMinutes.ToString(),
            ["Year"] = DateTime.UtcNow.Year.ToString(),
        });

        await notification.SendEmailAsync(
            to: email,
            subject: "Reset your Expense Tracker password",
            htmBody: body,
            cancellationToken: ct);
    }
}
