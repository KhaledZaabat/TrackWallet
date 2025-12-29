using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Contracts.Requests.Files;

public record UploadImageRequest(string EntityType, Guid EntityId, IFormFile Image);