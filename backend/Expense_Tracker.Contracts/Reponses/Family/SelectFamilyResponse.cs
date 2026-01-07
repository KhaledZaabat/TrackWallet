

using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Reponses.Transaction;

namespace Expense_Tracker.Contracts.Reponses.Family;

public sealed record SelectFamilyResponse(
    string UserId,
    string Email,
    string FullName,
    TokenResponse JwtToken,
    TokenResponse RefreshToken,
    FamilyContextDto FamilyContext,
    List<BudgetHistoryItem> BudgetHistory,
    List<TransactionItem> RecentTransactions,
    string? ProfileImageUrl = null
);


public sealed record FamilyWithMembersResponse(
    Guid Id,
    string Name,
    decimal CurrentBudget,
    string? FamilyBio,
    IReadOnlyList<FamilyUserProfileResponse> Members
);

public sealed record FamilyUserProfileResponse(
    Guid UserId,
    string FullName,
    string UserName,
    DateOnly? BirthDate,
    bool? IsMale,
    string? ProfileImageUrl,
    bool IsParent
);
