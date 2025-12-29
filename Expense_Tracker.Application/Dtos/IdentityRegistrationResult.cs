namespace Expense_Tracker.Application.Dtos;

public sealed record IdentityRegistrationResult(
    string IdentityUserId,
    string FullName,
    string Email,
    string Role
);
