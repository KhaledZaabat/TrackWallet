using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using System.Security.Claims;

namespace Expense_Tracker.App.Implemntation;

public sealed class HttpFamilyContext(IHttpContextAccessor accessor) : IFamilyContext
{
    public Guid? FamilyId
    {
        get
        {
            string? raw = accessor.HttpContext?
                .User?
                .FindFirstValue(CustomClaimTypes.FamilyId);

            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (!Guid.TryParse(raw, out Guid familyId))
                return null;

            return familyId;
        }
    }


    public bool IsParent
    {
        get
        {
            string? raw = accessor.HttpContext?
                .User?
                .FindFirstValue(CustomClaimTypes.IsParent);

            if (string.IsNullOrWhiteSpace(raw))
                return false;

            return bool.TryParse(raw, out bool isParent) && isParent;
        }
    }


    public bool HasFamilyContext => FamilyId.HasValue;
}
