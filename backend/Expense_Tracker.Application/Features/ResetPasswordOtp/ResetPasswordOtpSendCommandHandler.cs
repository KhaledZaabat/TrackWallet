
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Features.Identity.Commands.ForgotPassword;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

public class ResetPasswordOtpSendCommandHandler(IIdentityService _identityService, IPublisher _publisher)
    : IRequestHandler<ResetPasswordOtpSendCommand, Result>
{
    public async Task<Result> Handle(ResetPasswordOtpSendCommand request, CancellationToken cancellationToken)
    {
        Result<AuthenticatedUser> userResult =
            await _identityService.FindUserByEmailAsync(request.Email);

        if (userResult.IsFailure)
            return Result.Success();


        var user = userResult.TryGetValue();

        await _publisher.Publish(
          new ForgotPasswordEvent(
              Email: user.Email,
              UserName: user.UserName
          ),
          cancellationToken
      );

        return Result.Success();
    }
}
