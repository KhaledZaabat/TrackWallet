using ErrorOr;
using Wolverine;

using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;

public class ResetPasswordCommandHandler(IIdentityService _identityService, IMessageBus bus)
{

    public async Task<ErrorOr<Success>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        ErrorOr<AuthenticatedUser> userResult;

        userResult = await _identityService.FindUserByEmailAsync(request.Email);

        if (userResult.IsError)
            return userResult.Errors;

        AuthenticatedUser user = userResult.Value;

        ErrorOr<Success> resetResult = await _identityService.ResetPasswordAsync(user.Id, request.NewPassword, cancellationToken);

        if (resetResult.IsError) return resetResult;

        await bus.PublishAsync(
            new PasswordUpdatedEvent(
                Email: user.Email!,
                UserName: user.UserName!,
                IpAddress: request.UserIpAddress,
                Timestamp: DateTime.UtcNow
            )
        );

        return new Success();
    }
}
