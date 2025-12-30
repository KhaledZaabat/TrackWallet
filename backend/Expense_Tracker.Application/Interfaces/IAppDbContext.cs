
using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Files;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.TransactionFolder;
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
    #endregion

    #region Files
    DbSet<UploadedFile> UploadedFiles { get; }
    #endregion

    #region Notification Preferences
    DbSet<NotificationPreferences> NotificationPreferences { get; }
    #endregion

    #region Push Notifications
    DbSet<DomainNotification> Notifications { get; }
    DbSet<UserDevice> UserDevices { get; }
    #endregion

    #region Family
    DbSet<Family> Families { get; }
    DbSet<FamilyUser> FamilyUsers { get; }
    DbSet<FamilyBudgetHistory> FamilyBudgetHistories { get; }
    #endregion

    #region Invitations
    DbSet<Invitation> Invitations { get; }
    #endregion

    #region Categories
    DbSet<Category> Categories { get; }
    #endregion

    #region Transactions
    DbSet<Transaction> Transactions { get; }
    #endregion


    #region Persistence
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    int SaveChanges();
    DatabaseFacade Database { get; }
    #endregion
}