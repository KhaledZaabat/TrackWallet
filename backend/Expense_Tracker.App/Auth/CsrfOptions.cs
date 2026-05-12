using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Configuration for CSRF (anti-forgery) protection layered on top of the
/// cookie-based authentication transport.
/// </summary>
/// <remarks>
/// Bound from the <c>Csrf</c> configuration section via
/// <c>AddOptions&lt;CsrfOptions&gt;().BindConfiguration(CsrfOptions.SectionName)</c>
/// with <c>ValidateDataAnnotations().ValidateOnStart()</c>.
/// Satisfies Requirements 12.1, 12.5, 17.5, 22.6.
/// </remarks>
public sealed class CsrfOptions
{
    public const string SectionName = "Csrf";

    /// <summary>
    /// Name of the non-HttpOnly CSRF cookie that the frontend reads and echoes
    /// back in the <see cref="HeaderName"/> header on unsafe HTTP methods.
    /// </summary>
    [Required]
    public string CookieName { get; set; } = "XSRF-TOKEN";

    /// <summary>
    /// HTTP request header name that carries the CSRF token value on unsafe
    /// authenticated requests (POST/PUT/PATCH/DELETE).
    /// </summary>
    [Required]
    public string HeaderName { get; set; } = "X-XSRF-TOKEN";

    /// <summary>
    /// SameSite attribute applied to the CSRF cookie. Defaults to
    /// <see cref="SameSiteMode.Strict"/> as an additional CSRF mitigation layer
    /// (Requirement 22.6).
    /// </summary>
    public SameSiteMode SameSite { get; set; } = SameSiteMode.Strict;

    /// <summary>
    /// Request paths that bypass CSRF validation because no session cookie is
    /// established yet (pre-login endpoints) or because they are part of the
    /// session bootstrap itself. Requirement 12.5.
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
        };
}
