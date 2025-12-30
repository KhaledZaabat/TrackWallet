using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;

public sealed class NotificationPreferences : Entity
{
    public bool EmailNotifications { get; private set; }
    public bool PushNotifications { get; private set; }

    private NotificationPreferences() { }

    private NotificationPreferences(

        bool email,
        bool push)
    {
        EmailNotifications = email;
        PushNotifications = push;
    }


    private NotificationPreferences(
        Guid Id,
        bool email,
        bool push

       ) : base(Id)
    {
        EmailNotifications = email;
        PushNotifications = push;

    }

    public static Result<NotificationPreferences> Create(
        bool email,
        bool push
      )
    {
        if (!email && !push)
        {
            return Result.Failure<NotificationPreferences>(
                NotificationPreferencesError.InvalidState(
                    "At least one notification type must be enabled."
                ));
        }

        return Result.Success(
            new NotificationPreferences(email, push));
    }




    public static NotificationPreferences Default()
        => new(DefaultNotificationId,
            email: true,
            push: false);


    public static readonly Guid DefaultNotificationId =
        Guid.Parse("018f3f0d-9c2c-7ab1-96da-4b821eac09ff");
}