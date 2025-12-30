namespace Expense_Tracker.Contracts.Reponses.Identity;

public sealed record AuthResponse(
    string UserId,
    string Email,
    string FullName,
    TokenResponse JwtToken,
    TokenResponse RefreshToken,
    string? ProfileImageUrl = null
   );







