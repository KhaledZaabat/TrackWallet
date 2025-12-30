using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.PushNotifications.UpdateFcmToken;

public sealed class UpsertUserDeviceCommandHandler(
    IUserContext userContext,
    IUserDeviceRepository userDeviceRepository,
    IAppDbContext db
) : IRequestHandler<UpsertUserDeviceCommand, Result>
{
    public async Task<Result> Handle(
        UpsertUserDeviceCommand request,
        CancellationToken cancellationToken)
    {
        Guid? userId = userContext.UserId;

        if (userId is null)
            return Result.Failure(UserError.Unauthorized());

        await userDeviceRepository.UpsertAsync(
            userId.Value,
            request.FcmToken,
            Domain.PushNotifications.Enums.PushPlatform.Android,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}