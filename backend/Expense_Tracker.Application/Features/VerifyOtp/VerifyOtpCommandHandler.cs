using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.Identity.Commands.VerifyOtp;

public sealed class VerifyOtpCommandHandler(IOtpService otpService)
{

    public async Task<ErrorOr<Success>> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        string key = $"reset:{command.Email}";
        bool isValid = otpService.Validate(key, command.Otp, removeOnSuccess: true);

        if (!isValid)
            return DomainErrors.OtpErrors.InvalidOrExpired();

        return new Success();
    }

}
