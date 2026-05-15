namespace Expense_Tracker.Application.Features.FilesFolder.Dtos;

/// <summary>
/// Backend-internal upload request. <see cref="Content"/> is read once,
/// streamed to storage, and never buffered in memory.
/// </summary>
public sealed record UploadFileRequest(
    string EntityType,
    Guid EntityId,
    string Folder,
    string OriginalFileName,
    string ContentType,
    Stream Content,
    bool IsPrimary = false);

public sealed record UploadedFileInfo(Guid FileId, string ContentHash, long SizeInBytes);
