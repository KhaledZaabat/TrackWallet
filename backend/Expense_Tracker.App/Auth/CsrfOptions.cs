using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Configuration for the double-submit cookie CSRF protection. The SPA reads the
/// non-HttpOnly <see cref="CookieName"/> cookie and echoes its value in the
/// <see cref="HeaderName"/> request header on unsafe HTTP methods. The
/// <see cref="CsrfValidationMiddleware"/> validates that the two match.
/// </summary>
public sealed class CsrfOptions
{
    public const string SectionName = "Csrf";

    /// <summary>
    /// Name of the non-HttpOnly cookie carrying the CSRF token that the SPA reads.
    /// </summary>
    [Required]
    public string CookieName { get; set; } = "XSRF-TOKEN";

    /// <summary>
    /// Request header the SPA uses to echo the CSRF token back on unsafe requests.
    /// </summary>
    [Required]
    public string HeaderName { get; set; } = "X-XSRF-TOKEN";

    /// <summary>SameSite attribute for the CSRF cookie.</summary>
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Strict;

    /// <summary>
    /// Paths exempt from CSRF validation (pre-login endpoints that have no session yet).
    /// </summary>
    public string[] ExemptPaths { get; set; } =
        {
            "/api/identity/login",
            "/api/identity/refresh",
            "/api/identity/register",
            "/api/identity/confirm-account",
            "/api/identity/confirm-account/otp/resend",
            "/api/identity/reset-password",
            "/api/identity/reset-password/otp/send",
            "/api/identity/reset-password/otp/verify",
            "/jobs",
        };
}
