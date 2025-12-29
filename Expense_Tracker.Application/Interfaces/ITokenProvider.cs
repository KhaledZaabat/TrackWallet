
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using System.Security.Claims;

namespace Expense_Tracker.Application.Interfaces;

public interface ITokenProvider : IScopedService
{
    Task<Result<AuthResponse>> GenerateJwtTokenAsync(
            AuthenticatedUser user,
            string deviceId,
            CancellationToken ct = default);

    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}


