using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Features.Refresh.Dto;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

public interface IRefreshTokenService : IScopedService
{
    Task<Result<AuthenticatedUser>> GetUserFromRefreshTokenAsync(
        string refreshToken,
          string deviceId,
        CancellationToken ct);


    public Task<Result<RefreshTokenDto>> GetLatestAsync(
  Guid userId,
  string deviceId,
  CancellationToken ct);

    Task<Result> AddAsync(Guid userId, string token, string deviceId, CancellationToken ct = default);

    Task<Result> RevokeActiveTokensAsync(Guid userId, string deviceId, CancellationToken ct = default);

    Task<Result> RevokeAsync(string token, CancellationToken ct = default);
}