using Expense_Tracker.Application.Common.Errors;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Identity.Commands.VerifyOtp;

public sealed class VerifyOtpCommandHandler(IOtpService otpService) : IRequestHandler<VerifyOtpCommand, Result>
{


    public async Task<Result> Handle(VerifyOtpCommand command, CancellationToken cancellationToken)
    {
        string key = $"reset:{command.Email}";
        bool isValid = otpService.Validate(key, command.Otp, removeOnSuccess: true);

        if (!isValid)
            return Result.Failure(OtpError.InvalidOrExpired());

        return Result.Success();
    }

}