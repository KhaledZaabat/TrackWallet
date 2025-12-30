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
