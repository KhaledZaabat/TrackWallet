using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Expense_Tracker.Application.Constants;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.AcademicYearFolder;
using Expense_Tracker.Domain.AcademicYearFolder.CoursesFolder;
using Expense_Tracker.Domain.GroupFolder;
using Expense_Tracker.Domain.Users.Abstraction.NotificationPreferencesFolder;
using Expense_Tracker.Domain.Users.AdminFolder;
using Expense_Tracker.Infrastructure.Idenitity;

namespace Expense_Tracker.Infrastructure.Data.Seeding
{
    public class DbInitializer(
        UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager,
        IServiceProvider serviceProvider,
        IAppDbContext appDbContext
    ) : IDbInitializer
    {




        private async Task SeedAcademicYearsAndCoursesAsync()
        {
            if (appDbContext.AcademicYears.Any())
                return;

            Guid NewId() => Guid.CreateVersion7();

            // -----------------------------
            // Create Academic Years
            // -----------------------------
            var y1 = AcademicYear.Create(NewId(), "Y1").TryGetValue();
            var y2 = AcademicYear.Create(NewId(), "Y2").TryGetValue();
            var y3 = AcademicYear.Create(NewId(), "Y3").TryGetValue();
            var y4 = AcademicYear.Create(NewId(), "Y4").TryGetValue();

            appDbContext.AcademicYears.AddRange(y1, y2, y3, y4);

            // -----------------------------
            // Add Groups using domain rule
            // -----------------------------
            void AddGroups(AcademicYear year)
            {
                for (int i = 1; i <= 12; i++)
                {
                    var group = Group.Create(NewId(), $"G{i}").TryGetValue();

                    var update = group.UpdateAcademicYear(year);
                    if (update.IsFailure)
                        throw new Exception(update.TryGetError().Description);

                    appDbContext.Groups.Add(group);
                }
            }

            AddGroups(y1);
            AddGroups(y2);
            AddGroups(y3);
            AddGroups(y4);

            // ----------------------------------------
            // 3. COURSES PER YEAR (SEMESTERS MERGED)
            // ----------------------------------------

            // ---------------- Y1: Sem 1 + Sem 2 ----------------
            string[] y1Courses =
            {
        // Semester 1
        "Foundational Mathematics",
        "Data Structures and Algorithms 1",
        "Digital Systems",
        "Information Technology Essentials",
        "English 1",
        "Critical Thinking and Creativity Skills",

        // Semester 2
        "Object Oriented Programming",
        "Introduction to Linux",
        "Linear Algebra",
        "Mathematical Analysis 1",
        "Introduction to Statistics",
        "English 2"
    };

            // ---------------- Y2: Sem 3 + Sem 4 ----------------
            string[] y2Courses =
            {
        // Semester 3
        "Data Structures and Algorithms 2",
        "Mathematical Logic",
        "Mathematical Analysis 2",
        "Databases",
        "Probability",
        "Web Development",
        "Introduction to Business",

        // Semester 4
        "Theory of Computing",
        "Operating Systems",
        "Computer Architecture",
        "Statistical Inference",
        "Introduction to AI",
        "Mathematical Analysis 3",
        "Electronic Circuits Labs"
    };

            // ---------------- Y3: Sem 5 + Sem 6 ----------------
            string[] y3Courses =
            {
        // Semester 5
        "Data Mining",
        "Operations Research",
        "Stochastic Modelling and Simulation",
        "Software Engineering",
        "Networks and Protocols",
        "Mobile Development",
        "Entrepreneurship and Innovation",

        // Semester 6
        "Machine Learning",
        "Numerical Methods and Optimisation",
        "Time Series Analysis and Classification",
        "Advanced Databases",
        "Computer and Network Security",
        "Group-Project",
        "Project Management"
    };

            // ---------------- Y4: Sem 7 + Sem 8 ----------------
            string[] y4Courses =
            {
        // Semester 7
        "Natural Language Processing",
        "Deep Learning",
        "Human Computer Interaction",
        "Wireless Communication Networks and Systems",
        "Introduction to Mobile Robotics",
        "Internship Project",
        "AI and Ethics",
        "Selected topics in AI-Technology",

        // Semester 8
        "Computer Vision",
        "Big Data Analytics and Visualization",
        "Reinforcement Learning",
        "Speech Processing",
        "High Performance Computing",
        "Enterprise Computing",
        "Academic Communication and Research"
    };

            // -----------------------------
            // Add courses
            // -----------------------------
            void AddCourses(AcademicYear year, string[] titles)
            {
                foreach (var t in titles)
                {
                    var courseResult = Course.Create(NewId(), t, year);
                    if (courseResult.IsFailure)
                        continue;

                    var course = courseResult.TryGetValue();

                    // domain update ensures private setters respected
                    var update = course.UpdateAcademicYear(year);
                    if (update.IsFailure)
                        throw new Exception(update.TryGetError().Description);

                    appDbContext.Courses.Add(course);
                }
            }

            AddCourses(y1, y1Courses);
            AddCourses(y2, y2Courses);
            AddCourses(y3, y3Courses);
            AddCourses(y4, y4Courses);

            await appDbContext.SaveChangesAsync(CancellationToken.None);
        }







        private async Task SeedRolesAndPermissionsAsync()
        {
            if (roleManager.Roles.Any()) return;


            IReadOnlyList<string> roleNames = DefaultRoles.GetAll();

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {

                    await roleManager.CreateAsync(new ApplicationRole
                    {

                        Name = roleName,
                        ConcurrencyStamp = Guid.CreateVersion7().ToString(),


                    });
                }
            }
        }



        private async Task SeedSuperAdminAsync()
        {
            var config = serviceProvider.GetRequiredService<IConfiguration>();

            string SuperAdminEmail = config["SuperAdmin:Email"]!;
            string SuperAdminPass = config["SuperAdmin:Password"]!;


            var ownerUsers = await userManager.GetUsersInRoleAsync(DefaultRoles.SuperAdmin);
            if (ownerUsers.Any())
                return;

            var ownerIdentityResult = ApplicationUser.Create(SuperAdminEmail, DefaultUsers.SuperAdmin.FullName);
            if (ownerIdentityResult.IsFailure) throw new InvalidOperationException();
            var ownerIdentity = ownerIdentityResult.TryGetValue();
            ownerIdentity.EmailConfirmed = true;

            IdentityResult identityResult = await userManager.CreateAsync(ownerIdentity, SuperAdminPass);
            if (!identityResult.Succeeded)
                throw new Exception(string.Join(",", identityResult.Errors.Select(e => e.Description)));

            await userManager.AddToRoleAsync(ownerIdentity, DefaultRoles.SuperAdmin);


            var ownerDomain = Admin.Create(

                id: ownerIdentity.Id,
                fullName: DefaultUsers.SuperAdmin.FullName,
                email: SuperAdminEmail,
                fireEvent: false
            );


            Admin admin = ownerDomain.TryGetValue();

            appDbContext.Admins.Add(admin);
            await appDbContext.SaveChangesAsync(CancellationToken.None);
        }




        private async Task SeedNotificationPreferencesAsync()
        {
            if (appDbContext.NotificationPreferences.Any())
                return;


            NotificationPreferences defaultNotifications = NotificationPreferences.Default();
            appDbContext.NotificationPreferences.Add(defaultNotifications);



            await appDbContext.SaveChangesAsync(CancellationToken.None);
        }




        public async Task SeedAsync()
        {

            await SeedNotificationPreferencesAsync();
            await SeedAcademicYearsAndCoursesAsync();


            await SeedRolesAndPermissionsAsync();
            await SeedSuperAdminAsync();


        }
    }
}