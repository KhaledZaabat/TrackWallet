using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.Userr.UpdateProfile;

public sealed record UpdateProfileCommand(
    string? FullName,
    DateOnly? BirthDate,
    bool? IsMale,
    IFormFile? ProfileImage
);
