using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Single source of truth for building <see cref="CookieOptions"/> for the
/// authentication-related cookies (access, refresh, CSRF). All auth cookie
/// writes and clears in the application MUST go through this abstraction
/// so that <c>HttpOnly</c>, <c>Secure</c>, <c>SameSite</c>, <c>Path</c>,
/// <c>Domain</c>, and <c>Name</c> cannot drift between call sites (R17.1, R22.2).
/// </summary>
public interface IAuthCookieWriter
{
    /// <summary>
    /// Writes the HttpOnly access-token cookie with attributes derived from
    /// <c>AuthCookieOptions</c> and an explicit <paramref name="expiresAt"/>
    /// that matches the JWT <c>exp</c> claim.
    /// </summary>
    void WriteAccessCookie(HttpContext ctx, string accessToken, DateTimeOffset expiresAt);

    /// <summary>
    /// Writes the HttpOnly refresh-token cookie with attributes derived from
    /// <c>AuthCookieOptions</c> and an explicit <paramref name="expiresAt"/>
    /// equal to the rotation-extended refresh expiry.
    /// </summary>
    void WriteRefreshCookie(HttpContext ctx, string rawRefreshToken, DateTimeOffset expiresAt);

    /// <summary>
    /// Issues a fresh non-HttpOnly CSRF cookie plus its corresponding request token
    /// via <c>IAntiforgery.GetAndStoreTokens</c>.
    /// </summary>
    void IssueCsrfCookie(HttpContext ctx);

    /// <summary>
    /// Idempotently refreshes the CSRF cookie (re-issues if missing or stale).
    /// Safe to call on every authenticated response.
    /// </summary>
    void RefreshCsrfCookie(HttpContext ctx);

    /// <summary>
    /// Clears the access, refresh, and CSRF cookies by writing expired
    /// <c>Set-Cookie</c> headers whose <c>Name</c>, <c>Path</c>, <c>Domain</c>,
    /// <c>Secure</c>, <c>SameSite</c>, and <c>HttpOnly</c> match the values used
    /// on write so browsers actually delete them (R14.2, R22.9).
    /// </summary>
    void ClearAuthCookies(HttpContext ctx);

    /// <summary>
    /// Returns a read-only list of descriptors for every auth cookie this writer
    /// registers. Consumed only by the startup security validator (R22.8).
    /// </summary>
    IReadOnlyList<AuthCookieDescriptor> GetRegisteredDescriptors();
}

/// <summary>
/// Description of an auth cookie's attributes for startup security validation.
/// </summary>
public sealed record AuthCookieDescriptor(
    string Name,
    bool HttpOnly,
    bool Secure,
    SameSiteMode SameSite,
    string Path,
    string? Domain
);
