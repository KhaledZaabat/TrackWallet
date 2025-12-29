using Expense_Tracker.Domain.Users.Abstraction;
using Expense_Tracker.Domain.Users.StudentsFolder.Enums;

namespace Expense_Tracker.Contracts.Requests.Users;

public sealed record UserListItemDto(
    Guid Id,
    string FullName,
    string Email,
    Role Role,
    UserStatus Status
);