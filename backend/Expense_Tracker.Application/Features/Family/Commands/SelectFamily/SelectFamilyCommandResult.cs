using Expense_Tracker.Contracts.Reponses.Identity;

namespace Expense_Tracker.Application.Features.Family.Commands.SelectFamily;

/// <summary>
/// Internal handler result. The raw token material is consumed by the
/// controller (which writes it into HttpOnly cookies) and never crosses the
/// wire. The public <see cref="SelectFamilyResponse"/> derives from this.
/// </summary>
public sealed record SelectFamilyCommandResult(
    string UserId,
    string Email,
    string FullName,
    TokenResponse JwtToken,
    TokenResponse RefreshToken,
    FamilyContextDto FamilyContext);
