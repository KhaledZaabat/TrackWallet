using Expense_Tracker.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Expense_Tracker.App.Filters;

/// <summary>
/// Requires that the user has selected a family (has family claims in JWT)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireFamilyAttribute() : Attribute, IAuthorizationFilter
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

        Guid? familyId = familyContext.FamilyId;
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
        }
    }
}