using Expense_Tracker.Application.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Expense_Tracker.App.Filters;

/// <summary>
/// Requires that the user has selected a family (has family claims in JWT)
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
public class RequireFamilyAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var user = context.HttpContext.User;

        if (!user.Identity?.IsAuthenticated ?? true)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var familyId = user.FindFirst(CustomClaimTypes.FamilyId)?.Value;

        if (string.IsNullOrWhiteSpace(familyId))
        {
            context.Result = new ObjectResult(new
            {
                error = "NO_FAMILY_SELECTED",
                message = "Please select a family first."
            })
            {
                StatusCode = 403
            };
        }
    }
}
