using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Contracts.Requests.Files;

public record UploadFileRequest(string EntityType, Guid EntityId, IFormFile File);
