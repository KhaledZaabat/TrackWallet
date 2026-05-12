using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Double-submit cookie CSRF check: the SPA reads the CSRF token from a non-HttpOnly
/// cookie and echoes it back in the <see cref="CsrfOptions.HeaderName"/> header.
/// A matching header ↔ cookie pair on unsafe HTTP methods targeting authorized endpoints
/// proves same-origin intent, because a cross-origin attacker cannot read the cookie and
/// therefore cannot reproduce its value in a custom header.
///
/// This replaces ASP.NET Core's <c>IAntiforgery</c> token system, which binds tokens to
/// the claims-based user identity and is incompatible with a stateless bearer-over-cookie
/// SPA where the user transitions from anonymous (at login) to authenticated between
/// token issuance and validation.
/// </summary>
public sealed class CsrfValidationMiddleware
{
    private static readonly HashSet<string> UnsafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "POST",
        "PUT",
        "PATCH",
        "DELETE",
    };

    private readonly RequestDelegate _next;
    private readonly IOptionsMonitor<CsrfOptions> _csrfOpts;

    public CsrfValidationMiddleware(RequestDelegate next, IOptionsMonitor<CsrfOptions> csrfOpts)
    {
        _next = next;
        _csrfOpts = csrfOpts;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        CsrfOptions opts = _csrfOpts.CurrentValue;

        foreach (string exempt in opts.ExemptPaths)
        {
            if (ctx.Request.Path.StartsWithSegments(exempt, StringComparison.OrdinalIgnoreCase))
            {
                await _next(ctx);
                return;
            }
        }

        bool unsafeMethod = UnsafeMethods.Contains(ctx.Request.Method);
        bool requiresAuth = EndpointAuthInspector.RequiresAuthorization(ctx);

        if (!unsafeMethod || !requiresAuth)
        {
            await _next(ctx);
            return;
        }

        string? cookieToken = ctx.Request.Cookies[opts.CookieName];
        string? headerToken = ctx.Request.Headers[opts.HeaderName].ToString();

        if (
            string.IsNullOrEmpty(cookieToken)
            || string.IsNullOrEmpty(headerToken)
            || !FixedTimeEquals(cookieToken, headerToken)
        )
        {
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Response.ContentLength = 0;
            return;
        }

        await _next(ctx);
    }

    private static bool FixedTimeEquals(string a, string b)
    {
        if (a.Length != b.Length)
            return false;

        int diff = 0;
        for (int i = 0; i < a.Length; i++)
        {
            diff |= a[i] ^ b[i];
        }
        return diff == 0;
    }
}
