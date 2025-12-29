namespace Expense_Tracker.Contracts.Reponses.Identity;


public sealed record AuthResponse(
    string UserId,
    string Email,
    string FullName,
    string Role,
    TokenResponse JwtToken,
    TokenResponse RefreshToken);





