using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Common.Settings;

/// <summary>
/// Options that describe how the centralized <c>Auth_Cookie_Writer</c> builds
/// <see cref="CookieOptions"/> for authentication-related cookies (access,
/// refresh, and CSRF). Bound from the <see cref="SectionName"/> configuration
/// section and validated at startup via data annotations and additional rules
/// (for example, Access and Refresh cookie names must differ — R2.4, R3.4).
/// </summary>
public sealed class AuthCookieOptions
{
    /// <summary>
    /// Configuration section name used to bind this options class.
    /// </summary>
    public const string SectionName = "AuthCookies";

    /// <summary>
    /// Name of the HttpOnly access-token cookie.
    /// </summary>
    [Required]
    public string AccessCookieName { get; set; } = "tw.access";

    /// <summary>
    /// Name of the HttpOnly refresh-token cookie. MUST differ from
    /// <see cref="AccessCookieName"/> (R2.4, R3.4).
    /// </summary>
    [Required]
    public string RefreshCookieName { get; set; } = "tw.refresh";

    /// <summary>
    /// Name of the non-HttpOnly CSRF cookie readable by the SPA.
    /// </summary>
    [Required]
    public string CsrfCookieName { get; set; } = "XSRF-TOKEN";

    /// <summary>
    /// <c>SameSite</c> attribute applied to the access cookie. Defaults to
    /// <see cref="SameSiteMode.Strict"/> (R22.6).
    /// </summary>
    public SameSiteMode AccessSameSite { get; set; } = SameSiteMode.Strict;

    /// <summary>
    /// <c>SameSite</c> attribute applied to the refresh cookie. Defaults to
    /// <see cref="SameSiteMode.Strict"/> (R22.6).
    /// </summary>
    public SameSiteMode RefreshSameSite { get; set; } = SameSiteMode.Strict;

    /// <summary>
    /// <c>SameSite</c> attribute applied to the CSRF cookie. Defaults to
    /// <see cref="SameSiteMode.Strict"/>; MAY be set to <c>Lax</c> where
    /// cross-subdomain navigation is required (R22.6).
    /// </summary>
    public SameSiteMode CsrfSameSite { get; set; } = SameSiteMode.Strict;

    /// <summary>
    /// Explicit <c>Path</c> attribute for the access cookie (R22.7).
    /// </summary>
    [Required]
    public string AccessPath { get; set; } = "/";

    /// <summary>
    /// Explicit <c>Path</c> attribute for the refresh cookie, scoped to the
    /// identity endpoints that rotate it (R22.7).
    /// </summary>
    [Required]
    public string RefreshPath { get; set; } = "/api/identity";

    /// <summary>
    /// Explicit <c>Path</c> attribute for the CSRF cookie (R22.7).
    /// </summary>
    [Required]
    public string CsrfPath { get; set; } = "/";

    /// <summary>
    /// Optional explicit <c>Domain</c> attribute shared across auth cookies.
    /// When <see langword="null"/>, no <c>Domain</c> attribute is emitted
    /// (host-only cookie).
    /// </summary>
    public string? Domain { get; set; } = null;

    /// <summary>
    /// Development-only opt-out that permits <c>Secure=false</c> when running
    /// in the Development environment. MUST be <see langword="false"/> outside
    /// of Development; enforced by the startup cookie-security validator
    /// (R2.5, R22.2).
    /// </summary>
    public bool AllowInsecureInDevelopment { get; set; } = false;
}
