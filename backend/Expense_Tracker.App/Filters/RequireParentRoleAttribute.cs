using Expense_Tracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Expense_Tracker.App.Filters;

/// <summary>
/// Requires that the user is a parent in the selected family
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireParentRoleAttribute() : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var familyContext = context.HttpContext.RequestServices.GetRequiredService<IFamilyContext>();
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var familyId = familyContext.FamilyId;
        var isParent = familyContext.IsParent;

        if (familyId is null)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Authorization.NoFamilySelected",
                Detail = "Please select a family first.",
                Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
            };

            problemDetails.Extensions["ErrorCode"] = "NO_FAMILY_SELECTED";
            problemDetails.Extensions["ErrorType"] = "Authorization.NoFamilySelected";

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status403Forbidden,
                ContentTypes = { "application/problem+json" }
            };
            return;
        }

        if (!isParent)
        {
            var problemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Title = "Authorization.InsufficientPermissions",
                Detail = "This action requires parent privileges.",
                Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}"
            };

            problemDetails.Extensions["ErrorCode"] = "INSUFFICIENT_PERMISSIONS";
            problemDetails.Extensions["ErrorType"] = "Authorization.InsufficientPermissions";

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = StatusCodes.Status403Forbidden,
                ContentTypes = { "application/problem+json" }
            };
        }
    }
}