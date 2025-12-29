namespace Expense_Tracker.Contracts.Responses.Files;

public record UploadImagesResponse(int Count, List<Guid> ImageIds, List<string> ImageUrls);
