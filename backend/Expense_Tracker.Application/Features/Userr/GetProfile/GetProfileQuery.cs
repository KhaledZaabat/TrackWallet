namespace Expense_Tracker.Application.Features.Userr.GetProfile;

public sealed record GetProfileQuery;

public sealed record UserProfileResponse(
    Guid Id,
    string FullName,
    string UserName,
    string Email,
    DateOnly? BirthDate,
    bool? IsMale,
    string? ProfileImageUrl,
    bool EmailNotifications,
    bool PushNotifications
);