using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.Userr.GetProfile;

public sealed record GetProfileQuery : IRequest<Result<UserProfileResponse>>;



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