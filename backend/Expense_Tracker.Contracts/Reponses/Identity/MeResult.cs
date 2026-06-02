namespace Expense_Tracker.Contracts.Reponses.Identity;

public sealed record MeResult(
    Guid UserId,
    string Email,
    string UserName,
    string FullName,
    DateOnly? BirthDate,
    bool? IsMale,
    string? ProfileImageUrl
);