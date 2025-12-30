using Expense_Tracker.Domain.CategoryFolder;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.Identity;
using Expense_Tracker.Domain.FamilyFolder;
using Expense_Tracker.Domain.FamilyUser;
using Expense_Tracker.Domain.Files;
using Expense_Tracker.Domain.Invitation;
using Expense_Tracker.Domain.PushNotifications;
using Expense_Tracker.Domain.TransactionFolder;
using Expense_Tracker.Domain.Users;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using Expense_Tracker.Infrastructure.Idenitity;
using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Data;

public class AppDbContext
    (DbContextOptions<AppDbContext> options, IPublisher mediator)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IAppDbContext
{
    public bool DisableCreationAudit { get; set; } = false;
    public bool DisableUpdateAudit { get; set; } = false;
    public bool DisableSoftDeleting { get; set; } = false;
    public bool DisableDomainEvents { get; set; } = false;

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

    // ----------------------------
    // SaveChanges + Domain Events
    // ----------------------------
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        List<AggregateRoot> domainEntities = ChangeTracker.Entries()
            .Where(e => e.Entity is AggregateRoot root && root.DomainEvents.Count > 0)
            .Select(e => (AggregateRoot)e.Entity)
            .ToList();

        List<DomainEvent> domainEvents = domainEntities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        int result = await base.SaveChangesAsync(cancellationToken);

        if (!DisableDomainEvents && domainEvents.Count > 0)
        {
            foreach (DomainEvent domainEvent in domainEvents)
            {
                await mediator.Publish(domainEvent, cancellationToken);
            }

            foreach (AggregateRoot entity in domainEntities)
            {
                entity.ClearDomainEvents();
            }
        }

        return result;
    }

    // ----------------------------
    // Model Configs
    // ----------------------------
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Automatically load all configurations in assembly
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}