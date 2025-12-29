using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Contracts.Requests.Identity;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string UserName,
    string FullName,
    DateOnly BirthDate,
    bool IsMale,
    IFormFile ProfileImage);
