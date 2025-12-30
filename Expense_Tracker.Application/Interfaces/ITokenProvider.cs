using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using System.Security.Claims;

namespace Expense_Tracker.Application.Interfaces;


public interface ITokenProvider
{
    /// <summary>
    /// Generates JWT token without family context (for initial login)
    /// </summary>
    Task<Result<AuthDto>> GenerateJwtTokenAsync(
        AuthenticatedUser user,
        string deviceId,
        CancellationToken ct = default);

    /// <summary>
    /// Generates JWT token with family context (for family selection)
    /// </summary>
    Task<Result<AuthDto>> GenerateJwtTokenWithFamilyAsync(
        AuthenticatedUser user,
        string deviceId,
        FamilyContextDto? familyContext,
        CancellationToken ct = default);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);


}

