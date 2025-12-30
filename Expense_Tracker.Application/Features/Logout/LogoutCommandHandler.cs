using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
namespace Expense_Tracker.Application.Features.Identity.Commands.Logout;

public sealed class LogoutCommandHandler(IRefreshTokenService refreshTokens, IUserContext userContext, IUserDeviceRepository userDeviceRepository, IAppDbContext db)
    : IRequestHandler<LogoutCommand, Result>
{

    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        Guid? userId = userContext.UserId;

        if (userId is null)
            return Result.Failure(UserError.NotFound());


        await userDeviceRepository.UnbindDeviceAsync(request.FcmToken, ct);
        await db.SaveChangesAsync(ct);

        return await refreshTokens.RevokeActiveTokensAsync(userId.Value, request.DeviceId, ct);
    }
}
