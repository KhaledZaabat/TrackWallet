namespace Expense_Tracker.Contracts.Reponses.Identity;


public sealed record AuthDto(
    string UserId,
    string Email,
    string FullName,
    TokenResponse JwtToken,
    TokenResponse RefreshToken,
    FamilyContextDto? FamilyContext = null

   );





public sealed record FamilyContextDto(
    string FamilyId,
    string FamilyName,
    bool IsParent
);

