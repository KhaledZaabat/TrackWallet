using ErrorOr;
using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Domain.Files;

public sealed class UploadedFile : Entity
{
    public string FileName { get; private set; } = default!;
    public string StoredFileName { get; private set; } = default!;
    public string ContentType { get; private set; } = default!;
    public string FileExtension { get; private set; } = default!;

    public string EntityType { get; private set; } = default!;
    public Guid EntityId { get; private set; }

    public string Folder { get; private set; } = default!;
    public long FileSizeInBytes { get; private set; }

    /// <summary>
    /// Lower-case hex SHA-256 of the file bytes. Used as the strong
    /// <c>ETag</c> and to dedupe identical uploads.
    /// </summary>
    public string ContentHash { get; private set; } = default!;

    public bool IsPrimary { get; private set; }

    private UploadedFile() { }

    private UploadedFile(
        Guid id,
        string entityType,
        Guid entityId,
        string folder,
        string fileName,
        string storedFileName,
        string contentType,
        string fileExtension,
        long fileSize,
        string contentHash,
        bool isPrimary)
        : base(id)
    {
        EntityType = entityType;
        EntityId = entityId;
        Folder = folder;
        FileName = fileName;
        StoredFileName = storedFileName;
        ContentType = contentType;
        FileExtension = fileExtension;
        FileSizeInBytes = fileSize;
        ContentHash = contentHash;
        IsPrimary = isPrimary;
    }

    public static ErrorOr<UploadedFile> Create(
        string entityType,
        Guid entityId,
        string folder,
        string fileName,
        string storedFileName,
        string contentType,
        string fileExtension,
        long fileSize,
        string contentHash,
        bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return DomainErrors.GeneralErrors.InvalidState(nameof(UploadedFile), "EntityType is required.");

        if (entityId == Guid.Empty)
            return DomainErrors.GeneralErrors.InvalidState(nameof(UploadedFile), "EntityId is required.");

        if (string.IsNullOrWhiteSpace(fileName))
            return DomainErrors.GeneralErrors.InvalidState(nameof(UploadedFile), "FileName is required.");

        if (string.IsNullOrWhiteSpace(storedFileName))
            return DomainErrors.GeneralErrors.InvalidState(nameof(UploadedFile), "StoredFileName is required.");

        if (string.IsNullOrWhiteSpace(contentType))
            return DomainErrors.GeneralErrors.InvalidState(nameof(UploadedFile), "ContentType is required.");

        if (string.IsNullOrWhiteSpace(fileExtension))
            return DomainErrors.GeneralErrors.InvalidState(nameof(UploadedFile), "FileExtension is required.");

        if (string.IsNullOrWhiteSpace(contentHash))
            return DomainErrors.GeneralErrors.InvalidState(nameof(UploadedFile), "ContentHash is required.");

        if (!fileExtension.StartsWith("."))
            fileExtension = "." + fileExtension.ToLowerInvariant();

        if (fileSize <= 0)
            return DomainErrors.GeneralErrors.InvalidState(nameof(UploadedFile), "FileSize must be > 0.");

        return new UploadedFile(
            Guid.CreateVersion7(),
            entityType.Trim(),
            entityId,
            folder.Trim(),
            fileName.Trim(),
            storedFileName.Trim(),
            contentType.Trim(),
            fileExtension,
            fileSize,
            contentHash.Trim().ToLowerInvariant(),
            isPrimary);
    }

    public ErrorOr<Success> MarkAsPrimary()
    {
        IsPrimary = true;
        return new Success();
    }

    public ErrorOr<Success> UnmarkAsPrimary()
    {
        IsPrimary = false;
        return new Success();
    }
}
