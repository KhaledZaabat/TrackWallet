
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResendConfirmation;

public class ResendConfirmationCommandHandler(IIdentityService _identityService, IPublisher _publisher)
    : IRequestHandler<ResendConfirmationCommand, Result>
{




    public async Task<Result> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
    {

        //  Find user by email 
        Result<AuthenticatedUser> userResult =
             await _identityService.FindUserByEmailAsync(request.Email, requireConfirmedEmail: false);

        if (userResult.IsFailure)
            return Result.Success();


        Result isConfirmedResult = await _identityService.IsUserNotConfirmedAsync(request.Email, cancellationToken);
        if (isConfirmedResult.IsFailure)
            return isConfirmedResult;



        var user = userResult.TryGetValue();


        await _publisher.Publish(new ResendConfirmationEvent(user), cancellationToken);

        return Result.Success();
    }
}
