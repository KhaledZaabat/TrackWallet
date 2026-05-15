using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;

namespace Expense_Tracker.Infrastructure.Services;

/// <summary>
/// Default <see cref="IEmailLinkService"/>. Uses
/// <see cref="QueryHelpers.AddQueryString(string, IDictionary{string, string?})"/>
/// to escape email + token correctly so the link survives copy-paste, plain-text
/// email rendering, and odd characters in addresses (e.g. <c>+</c>).
/// </summary>
public sealed class EmailLinkService(IOptions<EmailLinkOptions> options) : IEmailLinkService
{
    public string BuildConfirmEmailLink(string email, string token) =>
        BuildLink(options.Value.ConfirmEmailPath, email, token);

    public string BuildResetPasswordLink(string email, string token) =>
        BuildLink(options.Value.ResetPasswordPath, email, token);

    private string BuildLink(string path, string email, string token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(token);

        string baseUrl = options.Value.FrontendBaseUrl.TrimEnd('/');
        string normalizedPath = path.StartsWith('/') ? path : "/" + path;

        return QueryHelpers.AddQueryString(
            $"{baseUrl}{normalizedPath}",
            new Dictionary<string, string?>
            {
                ["email"] = email,
                ["token"] = token,
            });
    }
}
