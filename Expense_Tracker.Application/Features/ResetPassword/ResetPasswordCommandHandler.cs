
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler(IIdentityService _identityService, IPublisher _publisher)
    : IRequestHandler<ResetPasswordCommand, Result>
{






    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        Result<AuthenticatedUser> userResult;


        userResult = await _identityService.FindUserByEmailAsync(request.Email);

        if (userResult.IsFailure)
            return userResult;




        AuthenticatedUser user = (userResult as SuccessResult<AuthenticatedUser>)!.Value;

        Result resetResult = await _identityService.ResetPasswordAsync(user.Id, request.NewPassword, cancellationToken);

        if (resetResult.IsFailure) return resetResult;
        await _publisher.Publish(
            new PasswordUpdatedEvent(
                Email: user.Email!,
                UserName: user.UserName!,
                IpAddress: request.UserIpAddress,
                Timestamp: DateTime.UtcNow

            ),
            cancellationToken
        );

        return Result.Success();
    }
}
