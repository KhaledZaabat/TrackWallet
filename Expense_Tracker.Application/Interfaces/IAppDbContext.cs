using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Expense_Tracker.Domain.AcademicYearFolder;
using Expense_Tracker.Domain.AcademicYearFolder.CoursesFolder;
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

public interface IAppDbContext
{
    bool DisableCreationAudit { get; set; }
    bool DisableUpdateAudit { get; set; }
    bool DisableSoftDeleting { get; set; }


    #region Users
    DbSet<User> Users { get; }

    DbSet<Student> Students { get; }
    DbSet<Admin> Admins { get; }
    DbSet<Instructor> Instructors { get; }
    #endregion

    #region Academic Structure
    DbSet<AcademicYear> AcademicYears { get; }
    DbSet<Course> Courses { get; }
    DbSet<Group> Groups { get; }
    DbSet<InstructorCourse> InstructorCourses { get; }
    DbSet<GroupInstructor> GroupInstructors { get; }
    DbSet<GroupStudent> GroupStudents { get; }
    #endregion

    #region Notification Preferences
    DbSet<NotificationPreferences> NotificationPreferences { get; }
    #endregion

    #region Quiz System (New)
    DbSet<Quiz> Quizzes { get; }
    DbSet<QuizQuestion> QuizQuestions { get; }

    // TPC derived sets
    DbSet<MultipleChoiceQuestion> MultipleChoiceQuestions { get; }
    DbSet<ShortAnswerQuestion> ShortAnswerQuestions { get; }

    DbSet<QuestionOption> QuestionOptions { get; }
    DbSet<QuizGroup> QuizGroups { get; }
    #endregion

    #region Persistence
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    int SaveChanges();
    Task DispatchDomainEventsAsync(CancellationToken cancellationToken);
    DatabaseFacade Database { get; }
    #endregion
}
