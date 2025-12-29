using System.Reflection;

namespace Expense_Tracker.Application.Constants;

public static class DefaultRoles
{

    public const string Admin = "Admin";
    public const string Instructor = "Instructor";
    public const string Student = "Student";
    public const string SuperAdmin = "SuperAdmin";


    public static IReadOnlyList<string> GetAll()
    {
        return typeof(DefaultRoles)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly && f.FieldType == typeof(string))
            .Select(f => f.GetRawConstantValue()!.ToString()!)
            .ToList();
    }
}
