using ErrorOr;
using Expense_Tracker.Application.Dtos;
using Expense_Tracker.Application.Events;
using Expense_Tracker.Application.Interfaces;
using Wolverine;

namespace Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;

/// <summary>
/// One-shot reset: token verification and the password change happen
/// atomically inside <see cref="IIdentityService.ResetPasswordWithTokenAsync"/>.
/// </summary>
public sealed class ResetPasswordCommandHandler(
    IIdentityService identityService,
    IMessageBus bus)
{
    public async Task<ErrorOr<Success>> Handle(
        ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        // Token verification + password change in one Identity round-trip.
        ErrorOr<Success> resetResult = await identityService.ResetPasswordWithTokenAsync(
            request.Email,
            request.Token,
            request.NewPassword,
            cancellationToken);

        if (resetResult.IsError)
            return resetResult.Errors;

        // Pull the user just for the post-event payload (best-effort).
        ErrorOr<AuthenticatedUser> userResult =
            await identityService.FindUserByEmailAsync(request.Email, requireConfirmedEmail: false);

        if (!userResult.IsError)
        {
            await bus.PublishAsync(new PasswordUpdatedEvent(
                Email: userResult.Value.Email!,
                UserName: userResult.Value.UserName!,
                IpAddress: request.UserIpAddress,
                Timestamp: DateTime.UtcNow));
        }

        return new Success();
    }
}
