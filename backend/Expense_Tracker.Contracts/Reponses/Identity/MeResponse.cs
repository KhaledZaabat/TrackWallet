namespace Expense_Tracker.Contracts.Reponses.Identity;

public sealed record MeResponse(
    Guid Id,
    string Email,
    string UserName,
    string FullName,
    DateOnly? BirthDate,
    bool? IsMale,
    string? ProfileImageUrl
);