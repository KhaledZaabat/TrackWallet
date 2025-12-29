namespace Expense_Tracker.Application.Features.FilesFolder.Dtos;

public record FileDto(byte[] Content, string ContentType, string FileName);
