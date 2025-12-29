using MediatR;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Events;

namespace Expense_Tracker.Application.Features.ForgotPassword;

public sealed class ForgetPasswordCommandHandler(
    IIdentityService identityService, IPublisher publisher
) : IRequestHandler<ForgetPasswordCommand, Result>
{
    public async Task<Result> Handle(
        ForgetPasswordCommand request,
        CancellationToken cancellationToken)
    {




        Result<AuthenticatedUser> result = await identityService.FindUserByEmailAsync(request.Email);
        if (result.IsFailure) return Result.Success();

        AuthenticatedUser user = result.TryGetValue();
        await publisher.Publish(new ResetPasswordEvent(user.Id, user.Email, user.FullName, user.Role));

        return Result.Success();

    }
}
