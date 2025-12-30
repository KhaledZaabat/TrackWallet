namespace Expense_Tracker.Domain.Users.Abstraction;

public enum Role
{
    Parent,   // Can manage expenses, add/remove family members, view reports
    Child,    // Can add/view their own expenses, request allowances
}