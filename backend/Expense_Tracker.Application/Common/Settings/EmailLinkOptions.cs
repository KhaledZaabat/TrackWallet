using System.ComponentModel.DataAnnotations;

namespace Expense_Tracker.Application.Common.Settings;

/// <summary>
/// Configuration for the front-end magic links sent by email (account
/// confirmation, password reset).
/// </summary>
/// <remarks>
/// Bound from the <c>EmailLinks</c> configuration section.
/// <code>
/// "EmailLinks": {
///   "FrontendBaseUrl": "https://localhost:4200",
///   "ConfirmEmailPath": "/auth/confirm",
///   "ResetPasswordPath": "/auth/reset-password"
/// }
/// </code>
/// In production point <c>FrontendBaseUrl</c> at the deployed SPA origin.
/// </remarks>
public sealed class EmailLinkOptions
{
    public const string SectionName = "EmailLinks";

    /// <summary>Absolute origin of the front-end SPA (no trailing slash).</summary>
    [Required, Url]
    public string FrontendBaseUrl { get; init; } = "https://localhost:4200";

    /// <summary>Path for the confirmation-link landing page.</summary>
    [Required]
    public string ConfirmEmailPath { get; init; } = "/confirm-email";

    /// <summary>Path for the password-reset landing page.</summary>
    [Required]
    public string ResetPasswordPath { get; init; } = "/auth/reset-password";
}
