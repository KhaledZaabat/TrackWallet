namespace Expense_Tracker.Contracts.Responses.Files;

public record UploadManyFilesResponse(int Count, List<Guid> FileIds);
