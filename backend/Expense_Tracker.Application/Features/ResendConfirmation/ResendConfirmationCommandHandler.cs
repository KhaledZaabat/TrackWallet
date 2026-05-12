using ErrorOr;
using Wolverine;

using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResendConfirmation;

public class ResendConfirmationCommandHandler(IIdentityService _identityService, IMessageBus bus)
{

    public async Task<ErrorOr<Success>> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
    {

        //  Find user by email 
        ErrorOr<AuthenticatedUser> userResult =
             await _identityService.FindUserByEmailAsync(request.Email, requireConfirmedEmail: false);

        if (userResult.IsError)
            return new Success();

        ErrorOr<Success> isConfirmedResult = await _identityService.IsUserNotConfirmedAsync(request.Email, cancellationToken);
        if (isConfirmedResult.IsError)
            return isConfirmedResult;

        var user = userResult.Value;

        await bus.PublishAsync(new ResendConfirmationEvent(user));

        return new Success();
    }
}
