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
            context.Result = new ObjectResult(new
            {
                error = "NO_FAMILY_SELECTED",
                message = "Please select a family first."
            })
            {
                StatusCode = 403
            };
            return;
        }



        if (!isParent)
        {
            context.Result = new ObjectResult(new
            {
                error = "INSUFFICIENT_PERMISSIONS",
                message = "This action requires parent privileges."
            })
            {
                StatusCode = 403
            };
        }
    }
}