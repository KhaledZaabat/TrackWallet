using ErrorOr;
using Wolverine;

using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Features.Identity.Commands.ForgotPassword;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;

public class ResetPasswordOtpSendCommandHandler(IIdentityService _identityService, IMessageBus bus)
{
    public async Task<ErrorOr<Success>> Handle(ResetPasswordOtpSendCommand request, CancellationToken cancellationToken)
    {
        ErrorOr<AuthenticatedUser> userResult =
            await _identityService.FindUserByEmailAsync(request.Email);

        if (userResult.IsError)
            return new Success();

        var user = userResult.Value;

        await bus.PublishAsync(
          new ForgotPasswordEvent(
              Email: user.Email,
              UserName: user.UserName
          )
      );

        return new Success();
    }
}
