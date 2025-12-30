using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using System.Security.Claims;

namespace Expense_Tracker.App.Implemntation;

public sealed class HttpFamilyContext(IHttpContextAccessor accessor) : IFamilyContext, IScopedService
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

    public string? FamilyName
    {
        get
        {
            return accessor.HttpContext?
                .User?
                .FindFirstValue(CustomClaimTypes.FamilyName);
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

    public FamilyContextDto? GetFamilyContext()
    {
        if (FamilyId is null)
            return null;

        return new FamilyContextDto(
            FamilyId: FamilyId.Value.ToString(),
            FamilyName: FamilyName ?? string.Empty,
            IsParent: IsParent
        );
    }

    public bool HasFamilyContext => FamilyId.HasValue;
}
