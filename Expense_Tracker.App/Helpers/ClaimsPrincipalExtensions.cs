using Expense_Tracker.Application.Constants;
using System.Security.Claims;

namespace Expense_Tracker.App.Helpers;

/// <summary>
/// Extension methods for accessing family context from ClaimsPrincipal
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static Guid? GetFamilyId(this ClaimsPrincipal principal)
    {
        var familyIdClaim = principal.FindFirst(CustomClaimTypes.FamilyId)?.Value;
        return Guid.TryParse(familyIdClaim, out var familyId) ? familyId : null;
    }

    public static string? GetFamilyName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(CustomClaimTypes.FamilyName)?.Value;
    }

    public static bool IsParent(this ClaimsPrincipal principal)
    {
        var isParentClaim = principal.FindFirst(CustomClaimTypes.IsParent)?.Value;
        return bool.TryParse(isParentClaim, out var isParent) && isParent;
    }

    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var userIdClaim = principal.FindFirst(CustomClaimTypes.UserId)?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }
}