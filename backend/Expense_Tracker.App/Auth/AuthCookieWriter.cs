using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Interfaces;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Centralised writer for every authentication-related cookie (access, refresh, CSRF).
/// All attributes (HttpOnly, Secure, SameSite, Path, Domain, Expires, Name) originate here
/// so they cannot drift between call sites. Direct <c>Response.Cookies.Append</c> /
/// <c>Response.Cookies.Delete</c> for auth cookies is forbidden elsewhere in the
/// application (R17.1, R22.2).
/// </summary>
public sealed class AuthCookieWriter(
    IOptionsMonitor<AuthCookieOptions> cookieOpts,
    IOptionsMonitor<CsrfOptions> csrfOpts,
    IWebHostEnvironment env,
    IAntiforgery antiforgery
) : IAuthCookieWriter, IScopedService
{
    public void WriteAccessCookie(HttpContext ctx, string accessToken, DateTimeOffset expiresAt)
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        CookieOptions opts = BuildAccessCookieOptions(c, expiresAt);
        ctx.Response.Cookies.Append(c.AccessCookieName, accessToken, opts);
    }

    public void WriteRefreshCookie(
        HttpContext ctx,
        string rawRefreshToken,
        DateTimeOffset expiresAt
    )
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        CookieOptions opts = BuildRefreshCookieOptions(c, expiresAt);
        ctx.Response.Cookies.Append(c.RefreshCookieName, rawRefreshToken, opts);
    }

    public void IssueCsrfCookie(HttpContext ctx)
    {
        // IAntiforgery.GetAndStoreTokens emits its own Set-Cookie for the request token.
        // We then re-assert our CsrfOptions-owned attributes by appending the same cookie name
        // with our explicit CookieOptions so the final header carries our Path/Domain/Secure/SameSite.
        AntiforgeryTokenSet tokens = antiforgery.GetAndStoreTokens(ctx);

        CsrfOptions csrf = csrfOpts.CurrentValue;
        AuthCookieOptions c = cookieOpts.CurrentValue;

        if (!string.IsNullOrEmpty(tokens.RequestToken))
        {
            CookieOptions opts = BuildCsrfCookieOptions(c, csrf);
            ctx.Response.Cookies.Append(csrf.CookieName, tokens.RequestToken, opts);
        }
    }

    public void RefreshCsrfCookie(HttpContext ctx) => IssueCsrfCookie(ctx);

    public void ClearAuthCookies(HttpContext ctx)
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        CsrfOptions csrf = csrfOpts.CurrentValue;

        // Use the same attributes (Path, Domain, Secure, SameSite, HttpOnly) with an expired
        // Expires / zero Max-Age so the browser actually deletes each cookie (R14.2, R22.9).
        DateTimeOffset epoch = DateTimeOffset.UnixEpoch;

        CookieOptions access = BuildAccessCookieOptions(c, epoch);
        CookieOptions refresh = BuildRefreshCookieOptions(c, epoch);
        CookieOptions csrfOut = BuildCsrfCookieOptions(c, csrf);
        csrfOut.Expires = epoch;

        ctx.Response.Cookies.Append(c.AccessCookieName, string.Empty, access);
        ctx.Response.Cookies.Append(c.RefreshCookieName, string.Empty, refresh);
        ctx.Response.Cookies.Append(csrf.CookieName, string.Empty, csrfOut);
    }

    public IReadOnlyList<AuthCookieDescriptor> GetRegisteredDescriptors()
    {
        AuthCookieOptions c = cookieOpts.CurrentValue;
        CsrfOptions csrf = csrfOpts.CurrentValue;
        bool secure = ResolveSecure(c);

        return
        [
            new AuthCookieDescriptor(
                Name: c.AccessCookieName,
                HttpOnly: true,
                Secure: secure,
                SameSite: c.AccessSameSite,
                Path: c.AccessPath,
                Domain: c.Domain
            ),
            new AuthCookieDescriptor(
                Name: c.RefreshCookieName,
                HttpOnly: true,
                Secure: secure,
                SameSite: c.RefreshSameSite,
                Path: c.RefreshPath,
                Domain: c.Domain
            ),
            new AuthCookieDescriptor(
                Name: csrf.CookieName,
                HttpOnly: false,
                Secure: secure,
                SameSite: csrf.SameSite,
                Path: c.CsrfPath,
                Domain: c.Domain
            ),
        ];
    }

    private CookieOptions BuildAccessCookieOptions(AuthCookieOptions c, DateTimeOffset expiresAt) =>
        new()
        {
            HttpOnly = true,
            Secure = ResolveSecure(c),
            SameSite = c.AccessSameSite,
            Path = c.AccessPath,
            Domain = c.Domain,
            Expires = expiresAt,
            IsEssential = true,
        };

    private CookieOptions BuildRefreshCookieOptions(
        AuthCookieOptions c,
        DateTimeOffset expiresAt
    ) =>
        new()
        {
            HttpOnly = true,
            Secure = ResolveSecure(c),
            SameSite = c.RefreshSameSite,
            Path = c.RefreshPath,
            Domain = c.Domain,
            Expires = expiresAt,
            IsEssential = true,
        };

    private CookieOptions BuildCsrfCookieOptions(AuthCookieOptions c, CsrfOptions csrf) =>
        new()
        {
            HttpOnly = false, // non-HttpOnly by contract so the SPA can echo it in X-XSRF-TOKEN (R12.2)
            Secure = ResolveSecure(c),
            SameSite = csrf.SameSite,
            Path = c.CsrfPath,
            Domain = c.Domain,
            IsEssential = true,
        };

    private bool ResolveSecure(AuthCookieOptions c) =>
        !(env.IsDevelopment() && c.AllowInsecureInDevelopment);
}
