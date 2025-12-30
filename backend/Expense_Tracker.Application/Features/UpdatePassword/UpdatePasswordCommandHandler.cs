using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.UpdatePassword;

public sealed class UpdatePasswordCommandHandler(IIdentityService _identityService, IPublisher _publisher, IUserContext userContext)
    : IRequestHandler<UpdatePasswordCommand, Result>
{


    public async Task<Result> Handle(
        UpdatePasswordCommand request,
        CancellationToken cancellationToken)
    {

        Guid? userId = userContext.UserId;
        if (userId is null) return Result.Failure(UserError.NotFound());

        var result = await _identityService.ChangePasswordAsync(
            userId.Value,
            request.CurrentPassword,
            request.NewPassword);

        if (result.IsFailure)
            return result;

        Result<AuthenticatedUser> userResult = await _identityService.GetUserByIdAsync(userId.Value);

        if (userResult.IsFailure)
            return Result.Failure(userResult.TryGetError());

        AuthenticatedUser user = userResult.TryGetValue();
        await _publisher.Publish(
           new PasswordUpdatedEvent(
               Email: user.Email!,
               UserName: user.UserName!,
               IpAddress: request.UserIpAddress,
               Timestamp: DateTime.UtcNow),
           cancellationToken
       );

        return Result.Success();
    }
}
