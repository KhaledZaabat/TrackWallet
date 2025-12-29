using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Contracts.Requests.Files;

public record UploadManyFilesRequest(string EntityType, Guid EntityId, IFormFileCollection Files);
