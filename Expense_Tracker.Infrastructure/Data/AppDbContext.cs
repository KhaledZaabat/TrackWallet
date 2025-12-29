using MediatR;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Domain.AcademicYearFolder;
using Expense_Tracker.Domain.AcademicYearFolder.CoursesFolder;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.Identity;
using Expense_Tracker.Domain.GroupFolder;
using Expense_Tracker.Domain.QuizesFolder;
using Expense_Tracker.Domain.QuizesFolder.Abstraction;
using Expense_Tracker.Domain.QuizesFolder.QuestionsFolder;
using Expense_Tracker.Domain.QuizesFolder.QuizGroupFolder;
using Expense_Tracker.Domain.QuizesFolder.QuizOptionFolder;
using Expense_Tracker.Domain.Users.Abstraction;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using Expense_Tracker.Domain.Users.AdminFolder;
using Expense_Tracker.Domain.Users.InstructorsFolders;
using Expense_Tracker.Domain.Users.StudentsFolder;
using Expense_Tracker.Infrastructure.Idenitity;

namespace Expense_Tracker.Infrastructure.Data;

public class AppDbContext
    (DbContextOptions<AppDbContext> options, IPublisher mediator) : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options), IAppDbContext
{

    public bool DisableCreationAudit { get; set; } = false;
    public bool DisableUpdateAudit { get; set; } = false;
    public bool DisableSoftDeleting { get; set; } = false;




    public DbSet<ApplicationUser> IdentityUsers => Set<ApplicationUser>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // ----------------------------
    // Users
    // ----------------------------
    public DbSet<User> Users => Set<User>();

    public DbSet<Student> Students => Set<Student>();
    public DbSet<Instructor> Instructors => Set<Instructor>();
    public DbSet<Admin> Admins => Set<Admin>();

    // ----------------------------
    // Settings
    // ----------------------------
    public DbSet<NotificationPreferences> NotificationPreferences => Set<NotificationPreferences>();

    // ----------------------------
    // Academic Structure
    // ----------------------------
    public DbSet<AcademicYear> AcademicYears => Set<AcademicYear>();
    public DbSet<Course> Courses => Set<Course>();

    // ----------------------------
    // Groups
    // ----------------------------
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<InstructorCourse> InstructorCourses => Set<InstructorCourse>();
    public DbSet<GroupInstructor> GroupInstructors => Set<GroupInstructor>();
    public DbSet<GroupStudent> GroupStudents => Set<GroupStudent>();

    // ----------------------------
    // Quiz System
    // ----------------------------
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<MultipleChoiceQuestion> MultipleChoiceQuestions => Set<MultipleChoiceQuestion>();
    public DbSet<ShortAnswerQuestion> ShortAnswerQuestions => Set<ShortAnswerQuestion>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();
    public DbSet<QuizGroup> QuizGroups => Set<QuizGroup>();



    // ----------------------------
    // SaveChanges + Domain Events
    // ----------------------------
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync(cancellationToken);
        return await base.SaveChangesAsync(cancellationToken);
    }

    public async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
    {
        var domainEntities = ChangeTracker.Entries()
            .Where(x => x.Entity is AggregateRoot root && root.DomainEvents.Any())
            .Select(x => (AggregateRoot)x.Entity)
            .ToList();

        var events = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        foreach (var domainEvent in events)
            await mediator.Publish(domainEvent, cancellationToken);

        domainEntities.ForEach(e => e.ClearDomainEvents());
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
