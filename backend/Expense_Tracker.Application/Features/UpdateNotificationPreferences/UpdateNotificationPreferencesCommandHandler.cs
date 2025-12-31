using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Users;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Application.Features.UpdateNotificationPreferences;

public sealed class UpdateNotificationPreferencesCommandHandler(IAppDbContext db)
    : IRequestHandler<UpdateNotificationPreferencesCommand, Result>
{
    public async Task<Result> Handle(
        UpdateNotificationPreferencesCommand request,
        CancellationToken cancellationToken)
    {
        // Get user with preferences
        User? user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user is null)
            return Result.Failure(DomainError.NotFound(nameof(User)));

        if (request.PushNotifications is false && request.EmailNotifications is false)
        {
            return Result.Failure(
                NotificationPreferencesError.InvalidState(
                    "At least one of preferences must be enabled"));
        }

        // Get or create notification preferences
        NotificationPreferences? preferences = await db.NotificationPreferences
            .FirstOrDefaultAsync(np => np.PushNotifications == request.PushNotifications && np.EmailNotifications == request.EmailNotifications, cancellationToken);// there already seeded

        if (preferences is null)
            Result.Failure(
                NotificationPreferencesError.InvalidState(
                    "There is error  preferences not found "));

        user.UpdateNotificationPreferences(preferences!.Id);

        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}