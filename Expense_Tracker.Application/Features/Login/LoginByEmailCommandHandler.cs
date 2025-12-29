using MediatR;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Features.Login;

public sealed class LoginCommandHandler(
    IIdentityService identityService,
    ITokenProvider tokenProvider
) : IRequestHandler<LoginCommand, Result<AuthResponse>>
{
    public async Task<Result<AuthResponse>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        Result<AuthenticatedUser> authResult =
            await identityService.AuthenticateByEmailAsync(
                request.Email,
                request.Password);

        if (authResult.IsFailure)
            return Result.Failure<AuthResponse>(authResult.TryGetError());

        AuthenticatedUser user = authResult.TryGetValue();

        Result<AuthResponse> tokenResult =
            await tokenProvider.GenerateJwtTokenAsync(
                user,
                request.DeviceId,
                cancellationToken);

        if (tokenResult.IsFailure)
            return Result.Failure<AuthResponse>(tokenResult.TryGetError());

        return tokenResult;
    }
}
