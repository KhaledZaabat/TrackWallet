using System.Security.Cryptography;
using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Single source of truth for every authentication-related cookie (access token,
/// refresh token, CSRF double-submit token). All <see cref="CookieOptions"/> live
/// here so attributes cannot drift between write and clear call-sites.
/// </summary>
public sealed class AuthCookieWriter(
    IOptionsMonitor<AuthCookieOptions> cookieOpts,
    IOptionsMonitor<CsrfOptions> csrfOpts,
    IWebHostEnvironment env
) : IAuthCookieWriter, IScopedService
{
    public void WriteAccessCookie(HttpContext ctx, string accessToken, DateTimeOffset expiresAt)
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        ctx.Response.Cookies.Append(c.AccessCookieName, accessToken, BuildAccessOpts(c, expiresAt));
    }

    public void WriteRefreshCookie(
        HttpContext ctx,
        string rawRefreshToken,
        DateTimeOffset expiresAt
    )
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        ctx.Response.Cookies.Append(
            c.RefreshCookieName,
            rawRefreshToken,
            BuildRefreshOpts(c, expiresAt)
        );
    }

    /// <summary>
    /// Issues a cryptographically random CSRF token as a non-HttpOnly cookie. The SPA
    /// reads this cookie and echoes its value in the <see cref="CsrfOptions.HeaderName"/>
    /// header on unsafe requests. <see cref="CsrfValidationMiddleware"/> validates that
    /// the header matches the cookie (double-submit cookie pattern, OWASP recommended).
    /// No ASP.NET Core <c>IAntiforgery</c> involvement — no user-identity binding issues.
    /// </summary>
    public void IssueCsrfCookie(HttpContext ctx)
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        CsrfOptions csrf = csrfOpts.CurrentValue;

        string token = GenerateCsrfToken();
        ctx.Response.Cookies.Append(csrf.CookieName, token, BuildCsrfOpts(c, csrf));
    }

    public void RefreshCsrfCookie(HttpContext ctx) => IssueCsrfCookie(ctx);

    public void ClearAuthCookies(HttpContext ctx)
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        CsrfOptions csrf = csrfOpts.CurrentValue;
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        ctx.Response.Cookies.Append(c.AccessCookieName, string.Empty, BuildAccessOpts(c, epoch));
        ctx.Response.Cookies.Append(c.RefreshCookieName, string.Empty, BuildRefreshOpts(c, epoch));

        CookieOptions csrfClear = BuildCsrfOpts(c, csrf);
        csrfClear.Expires = epoch;
        ctx.Response.Cookies.Append(csrf.CookieName, string.Empty, csrfClear);
    }

    public IReadOnlyList<AuthCookieDescriptor> GetRegisteredDescriptors()
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        CsrfOptions csrf = csrfOpts.CurrentValue;
        bool secure = ResolveSecure(c);

        return
        [
            new(c.AccessCookieName, true, secure, c.AccessSameSite, c.AccessPath, c.Domain),
            new(c.RefreshCookieName, true, secure, c.RefreshSameSite, c.RefreshPath, c.Domain),
            new(csrf.CookieName, false, secure, csrf.SameSite, c.CsrfPath, c.Domain),
        ];
    }

    private CookieOptions BuildAccessOpts(AuthCookieOptions c, DateTimeOffset exp) =>
        new()
        {
            HttpOnly = true,
            Secure = ResolveSecure(c),
            SameSite = c.AccessSameSite,
            Path = c.AccessPath,
            Domain = c.Domain,
            Expires = exp,
            IsEssential = true,
        };

    private CookieOptions BuildRefreshOpts(AuthCookieOptions c, DateTimeOffset exp) =>
        new()
        {
            HttpOnly = true,
            Secure = ResolveSecure(c),
            SameSite = c.RefreshSameSite,
            Path = c.RefreshPath,
            Domain = c.Domain,
            Expires = exp,
            IsEssential = true,
        };

    private CookieOptions BuildCsrfOpts(AuthCookieOptions c, CsrfOptions csrf) =>
        new()
        {
            HttpOnly = false,
            Secure = ResolveSecure(c),
            SameSite = csrf.SameSite,
            Path = c.CsrfPath,
            Domain = c.Domain,
            IsEssential = true,
        };

    private bool ResolveSecure(AuthCookieOptions c) =>
        !(env.IsDevelopment() && c.AllowInsecureInDevelopment);

    private static string GenerateCsrfToken() =>
        Convert
            .ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
