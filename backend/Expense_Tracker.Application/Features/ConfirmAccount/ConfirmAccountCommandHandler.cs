using ErrorOr;
using Expense_Tracker.Application.Interfaces;

namespace Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;

public sealed class ConfirmAccountCommandHandler(IIdentityService identityService)
{
    public async Task<ErrorOr<Success>> Handle(ConfirmAccountCommand request, CancellationToken ct)
    {
        ErrorOr<Guid> result = await identityService.ConfirmEmailWithTokenAsync(
            request.Email, request.Token, ct);

        return result.IsError ? result.Errors : new Success();
    }
}
