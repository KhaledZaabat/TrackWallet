namespace Expense_Tracker.Application.Constants;

public static class RoleGroups
{
    /// <summary>
    /// Roles that have full administrative privileges.
    /// Includes: Admin, SuperAdmin.
    /// </summary>
    public const string Admins =
        $"{DefaultRoles.Admin},{DefaultRoles.SuperAdmin}";

    /// <summary>
    /// Roles that are considered system staff.
    /// Includes: Admin, SuperAdmin, Instructor.
    /// </summary>
    public const string Staff =
        $"{DefaultRoles.Admin},{DefaultRoles.SuperAdmin},{DefaultRoles.Instructor}";

    /// <summary>
    /// Roles that can teach or manage academic content.
    /// Includes: Instructor, Admin, SuperAdmin.
    /// </summary>
    public const string Teaching =
        $"{DefaultRoles.Instructor}";

    /// <summary>
    /// All roles in the system as a comma-separated list.
    /// Useful for broad authorization.
    /// </summary>
    public static string AllRoles =>
        string.Join(",", DefaultRoles.GetAll());
}
