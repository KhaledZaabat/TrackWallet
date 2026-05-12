using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;

public sealed class ConfirmAccountCommandHandler(IOtpService _otpService, IIdentityService IdentityService)
{

    public async Task<ErrorOr<Success>> Handle(ConfirmAccountCommand request, CancellationToken ct)
    {
        string key = $"confirm:{request.Email.ToLowerInvariant()}";

        bool valid = _otpService.Validate(key, request.Otp);

        if (!valid)
            return DomainErrors.OtpErrors.InvalidOrExpired();

        // Mark user as confirmed (email )
        ErrorOr<Guid> res = await IdentityService.ConfirmUserAsync(request.Email, ct);
        if (res.IsError)
            return res.Errors;

        return new Success();
    }
}
