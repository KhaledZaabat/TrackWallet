using Expense_Tracker.Domain.Common;
using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using System;
using System.Collections.Generic;
using System.Text;

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
    /// Indicates whether this image is the primary one for the entity.
    /// </summary>
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
        IsPrimary = isPrimary;
    }

    public static Result<UploadedFile> Create(
        string entityType,
        Guid entityId,
        string folder,
        string fileName,
        string storedFileName,
        string contentType,
        string fileExtension,
        long fileSize,
        bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            return Result.Failure<UploadedFile>(DomainError.InvalidState(nameof(UploadedFile), "EntityType is required."));

        if (entityId == Guid.Empty)
            return Result.Failure<UploadedFile>(DomainError.InvalidState(nameof(UploadedFile), "EntityId is required."));

        if (string.IsNullOrWhiteSpace(fileName))
            return Result.Failure<UploadedFile>(DomainError.InvalidState(nameof(UploadedFile), "FileName is required."));

        if (string.IsNullOrWhiteSpace(storedFileName))
            return Result.Failure<UploadedFile>(DomainError.InvalidState(nameof(UploadedFile), "StoredFileName is required."));

        if (string.IsNullOrWhiteSpace(contentType))
            return Result.Failure<UploadedFile>(DomainError.InvalidState(nameof(UploadedFile), "ContentType is required."));

        if (string.IsNullOrWhiteSpace(fileExtension))
            return Result.Failure<UploadedFile>(DomainError.InvalidState(nameof(UploadedFile), "FileExtension is required."));

        if (!fileExtension.StartsWith("."))
            fileExtension = "." + fileExtension.ToLowerInvariant();

        if (fileSize <= 0)
            return Result.Failure<UploadedFile>(DomainError.InvalidState(nameof(UploadedFile), "FileSize must be > 0."));

        return Result.Success(new UploadedFile(
            Guid.CreateVersion7(),
            entityType.Trim(),
            entityId,
            folder.Trim(),
            fileName.Trim(),
            storedFileName.Trim(),
            contentType.Trim(),
            fileExtension,
            fileSize,
            isPrimary));
    }

    public Result MarkAsPrimary()
    {
        IsPrimary = true;
        return Result.Success();
    }

    public Result UnmarkAsPrimary()
    {
        IsPrimary = false;
        return Result.Success();
    }
}
