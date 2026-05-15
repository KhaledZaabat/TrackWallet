using Expense_Tracker.Contracts.Reponses.Identity;

namespace Expense_Tracker.Contracts.Reponses.Family;

/// <summary>
/// Response for <c>POST /api/families/{familyId}/select</c>. Returns just the
/// family context the SPA needs to update its UI; transactions, budget history,
/// and members are fetched separately from the dedicated resource endpoints.
/// </summary>
/// <remarks>
/// Auth tokens are not in the body — they ride in HttpOnly cookies set by the
/// controller after the command succeeds.
/// </remarks>
public sealed record SelectFamilyResponse(
    string UserId,
    string Email,
    string FullName,
    FamilyContextDto FamilyContext);

public sealed record FamilyWithMembersResponse(
    Guid Id,
    string Name,
    decimal CurrentBudget,
    string? FamilyBio,
    IReadOnlyList<FamilyUserProfileResponse> Members);

public sealed record FamilyUserProfileResponse(
    Guid UserId,
    string FullName,
    string UserName,
    DateOnly? BirthDate,
    bool? IsMale,
    string? ProfileImageUrl,
    bool IsParent);
