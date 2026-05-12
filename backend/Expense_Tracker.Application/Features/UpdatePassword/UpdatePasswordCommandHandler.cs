using ErrorOr;
using Wolverine;

using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.UpdatePassword;

public sealed class UpdatePasswordCommandHandler(IIdentityService _identityService, IMessageBus bus, IUserContext userContext)
{

    public async Task<ErrorOr<Success>> Handle(
        UpdatePasswordCommand request,
        CancellationToken cancellationToken)
    {

        Guid? userId = userContext.UserId;
        if (userId is null) return DomainErrors.UserErrors.NotFound();

        ErrorOr<Success> result = await _identityService.ChangePasswordAsync(
            userId.Value,
            request.CurrentPassword,
            request.NewPassword);

        if (result.IsError)
            return result;

        ErrorOr<AuthenticatedUser> userResult = await _identityService.GetUserByIdAsync(userId.Value);

        if (userResult.IsError)
            return userResult.Errors;

        AuthenticatedUser user = userResult.Value;
        await bus.PublishAsync(
           new PasswordUpdatedEvent(
               Email: user.Email!,
               UserName: user.UserName!,
               IpAddress: request.UserIpAddress,
               Timestamp: DateTime.UtcNow)
       );

        return new Success();
    }
}
