using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.Identity;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUserFolder;
using Expense_Tracker.Domain.Files;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Domain.Users;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using Expense_Tracker.Infrastructure.Idenitity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Data;

public class AppDbContext
    (DbContextOptions<AppDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    public bool DisableCreationAudit { get; set; } = false;
    public bool DisableUpdateAudit { get; set; } = false;
    public bool DisableSoftDeleting { get; set; } = false;

    #region Identity
    public DbSet<ApplicationUser> IdentityUsers => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    #endregion

    #region Users
    public DbSet<User> Users => Set<User>();
    #endregion

    #region Notification Preferences
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();
    #endregion

    #region Files
    public DbSet<UploadedFile> UploadedFiles => Set<UploadedFile>();
    #endregion

    #region Push Notifications
    public DbSet<DomainNotification> Notifications => Set<DomainNotification>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    #endregion

    #region Family
    public DbSet<Family> Families => Set<Family>();
    public DbSet<FamilyUser> FamilyUsers => Set<FamilyUser>();
    public DbSet<FamilyBudgetHistory> FamilyBudgetHistories => Set<FamilyBudgetHistory>();
    #endregion

    #region Invitations
    public DbSet<Invitation> Invitations => Set<Invitation>();
    #endregion


    #region Categories

    public DbSet<Category> Categories => Set<Category>();
    #endregion
    #region Transactions
    public DbSet<Transaction> Transactions => Set<Transaction>();
    #endregion

   
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
