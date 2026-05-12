using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Validates the CSRF header on unsafe HTTP methods targeting authorized endpoints.
/// Runs between <c>SilentRefreshMiddleware</c> and <c>UseAuthorization</c>; short-circuits
/// with <c>403 Forbidden</c> on validation failure without emitting any cookies or
/// touching the auth rotation pipeline (R12.3, R12.4, R12.5).
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
    private readonly IAntiforgery _antiforgery;
    private readonly IOptionsMonitor<CsrfOptions> _csrfOpts;

    public CsrfValidationMiddleware(
        RequestDelegate next,
        IAntiforgery antiforgery,
        IOptionsMonitor<CsrfOptions> csrfOpts
    )
    {
        _next = next;
        _antiforgery = antiforgery;
        _csrfOpts = csrfOpts;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        CsrfOptions opts = _csrfOpts.CurrentValue;

        // R12.5 — exempt paths (login, register, refresh, password flows, etc.).
        foreach (string exempt in opts.ExemptPaths)
        {
            if (ctx.Request.Path.StartsWithSegments(exempt, StringComparison.OrdinalIgnoreCase))
            {
                await _next(ctx);
                return;
            }
        }

        // R12.3 — only validate on unsafe methods AND when the endpoint requires authorization.
        bool unsafeMethod = UnsafeMethods.Contains(ctx.Request.Method);
        bool requiresAuth = EndpointAuthInspector.RequiresAuthorization(ctx);

        if (!unsafeMethod || !requiresAuth)
        {
            await _next(ctx);
            return;
        }

        try
        {
            await _antiforgery.ValidateRequestAsync(ctx);
        }
        catch (AntiforgeryValidationException)
        {
            // R12.4 — short-circuit with 403 and no additional cookies / no rotation.
            ctx.Response.StatusCode = StatusCodes.Status403Forbidden;
            ctx.Response.ContentLength = 0;
            return;
        }

        await _next(ctx);
    }
}
