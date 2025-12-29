using MediatR;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Features.ResetPassword;

public sealed class ResetPasswordWithCodeCommandHandler(
    IIdentityService identityService, IPublisher publisher
) : IRequestHandler<ResetPasswordWithCodeCommand, Result>
{
    public async Task<Result> Handle(
        ResetPasswordWithCodeCommand request,
        CancellationToken cancellationToken)
    {

        var userResult = await identityService.GetUserByIdAsync(request.UserId);
        if (userResult.IsFailure)
            return Result.Failure(userResult.TryGetError());

        var resetResult = await identityService.ResetPasswordWithCodeAsync(
            request.UserId,
            request.Code,
            request.NewPassword,
            cancellationToken
        );
        if (resetResult.IsFailure) return resetResult;

        var user = userResult.TryGetValue();
        await publisher.Publish(
          new PasswordUpdatedEvent(
              Email: user.Email!,
              FullName: user.FullName!,
              IpAddress: request.UserIpAddress,
              Timestamp: DateTime.UtcNow),
          cancellationToken
      );

        return resetResult;
    }
}