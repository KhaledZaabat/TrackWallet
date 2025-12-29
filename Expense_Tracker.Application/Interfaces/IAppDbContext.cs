
using Expense_Tracker.Domain.Files;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.Users;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

public interface IAppDbContext
{
    bool DisableCreationAudit { get; set; }
    bool DisableUpdateAudit { get; set; }
    bool DisableSoftDeleting { get; set; }
    bool DisableDomainEvents { get; set; }

    #region Users
    DbSet<User> Users { get; }


    #region Files
    DbSet<UploadedFile> UploadedFiles { get; }

    #endregion

    #region Notification Preferences
    DbSet<NotificationPreferences> NotificationPreferences { get; }

    #endregion

    #endregion

    #region Push Notifications
    DbSet<DomainNotification> Notifications { get; }
    DbSet<UserDevice> UserDevices { get; }

    #endregion



    #region Persistence
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    int SaveChanges();
    Task DispatchDomainEventsAsync(CancellationToken cancellationToken);
    DatabaseFacade Database { get; }
    #endregion
}

