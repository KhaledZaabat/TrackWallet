using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string UserName,
    string FullName,
    DateOnly BirthDate,
    bool IsMale,
    IFormFile? ProfileImage);
