using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.App.Auth;
public static class EndpointAuthInspector
{
    public static bool RequiresAuthorization(HttpContext ctx)
    {
        var endpoint = ctx.GetEndpoint();
        if (endpoint is null)
            return false;
        if (endpoint.Metadata.GetMetadata<IAllowAnonymous>() is not null)
            return false; 
        return endpoint.Metadata.GetMetadata<IAuthorizeData>() is not null; 
    }
}
