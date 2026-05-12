using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.App.Auth;

/// <summary>
/// Reusable helper that decides whether an <see cref="HttpContext"/>'s matched endpoint
/// requires authorization. Consumed by <c>SilentRefreshMiddleware</c> and
/// <c>CsrfValidationMiddleware</c> so they share one decision (R6.1, R6.2, R6.3, R6.5).
/// </summary>
public static class EndpointAuthInspector
{
    /// <summary>
    /// Returns <see langword="true"/> iff the matched endpoint declares
    /// <see cref="IAuthorizeData"/> metadata and does NOT declare
    /// <see cref="IAllowAnonymous"/>. Returns <see langword="false"/> when there is
    /// no matched endpoint.
    /// </summary>
    public static bool RequiresAuthorization(HttpContext ctx)
    {
        var endpoint = ctx.GetEndpoint();
        if (endpoint is null)
            return false; // R6.1
        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            return false; // R6.2
        return endpoint.Metadata.GetMetadata<IAuthorizeData>() is not null; // R6.3
    }
}
