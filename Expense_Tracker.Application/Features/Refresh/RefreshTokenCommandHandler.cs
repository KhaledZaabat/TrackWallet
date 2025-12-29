using MediatR;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Features.Refresh;

public sealed class RefreshTokenCommandHandler(
    IRefreshTokenService refreshTokenService,
    ITokenProvider tokenProvider
) : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var userResult = await refreshTokenService.GetUserFromRefreshTokenAsync(
            request.RefreshToken,
            request.DeviceId,
            cancellationToken
        );

        if (userResult.IsFailure)
            return Result.Failure<AuthResponse>(userResult.TryGetError());

        var user = userResult.TryGetValue();

        Result<AuthResponse> tokenResult = await tokenProvider.GenerateJwtTokenAsync(user, request.DeviceId, cancellationToken);


        return tokenResult;
    }
}