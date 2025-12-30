using Expense_Tracker.Contracts.Reponses.Family;

namespace Expense_Tracker.Contracts.Reponses.Identity;

public sealed record AuthResponse(
    string UserId,
    string Email,
    string FullName,
    TokenResponse JwtToken,
    TokenResponse RefreshToken,
    List<FamilyResponse>? Families = null,
    string? ProfileImageUrl = null
   );







