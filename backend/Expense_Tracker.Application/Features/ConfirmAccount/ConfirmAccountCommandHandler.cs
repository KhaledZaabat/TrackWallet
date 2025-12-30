using Expense_Tracker.Application.Common.Errors;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;

public sealed class ConfirmAccountCommandHandler(IOtpService _otpService, IIdentityService IdentityService, IAppDbContext db) : IRequestHandler<ConfirmAccountCommand, Result>
{


    public async Task<Result> Handle(ConfirmAccountCommand request, CancellationToken ct)
    {
        string key = $"confirm:{request.Email.ToLowerInvariant()}";



        bool valid = _otpService.Validate(key, request.Otp);

        if (!valid)
            return Result.Failure(OtpError.InvalidOrExpired());

        // Mark user as confirmed (email )
        Result<Guid> res = await IdentityService.ConfirmUserAsync(request.Email, ct);
        if (res.IsFailure)
            return Result.Failure(res.TryGetError());

        return Result.Success();
    }
}