using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.Files;
using Expense_Tracker.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Services;

/// <summary>
/// Default <see cref="IFileService"/>. Coordinates the physical
/// <see cref="IFileStorage"/> and the <c>UploadedFiles</c> DB rows so callers
/// see exactly one consistent unit of work.
/// </summary>
public sealed class FileService(
    AppDbContext db,
    IFileStorage storage)
    : IFileService, IScopedService
{
    
    private static readonly System.Text.RegularExpressions.Regex SafeFolderPattern =
        new("^[A-Za-z0-9._/-]+$", System.Text.RegularExpressions.RegexOptions.Compiled);

    public Task<ErrorOr<UploadedFileInfo>> UploadAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile file,
        bool isPrimary = false,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return Task.FromResult<ErrorOr<UploadedFileInfo>>(DomainErrors.FileErrors.Empty());

        return UploadAsync(
            new UploadFileRequest(
                EntityType: entityType,
                EntityId: entityId,
                Folder: folder,
                OriginalFileName: file.FileName,
                ContentType: file.ContentType,
                Content: file.OpenReadStream(),
                IsPrimary: isPrimary),
            ct);
    }

    public async Task<ErrorOr<UploadedFileInfo>> UploadAsync(
        UploadFileRequest request,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.Content is null)
            return DomainErrors.FileErrors.Empty();

        if (string.IsNullOrWhiteSpace(request.Folder) || !SafeFolderPattern.IsMatch(request.Folder))
            return DomainErrors.FileErrors.InvalidType("Folder name contains invalid characters.");

        string originalName = Path.GetFileName(request.OriginalFileName ?? string.Empty);
        string ext = Path.GetExtension(originalName).ToLowerInvariant();
        string storedName = $"{Guid.CreateVersion7():N}{ext}";
        string storageKey = BuildKey(request.Folder, storedName);

     
        long size;
        string hash;

        await using (var hashing = new HashingStream(request.Content))
        {
            try
            {
                size = await storage.SaveAsync(storageKey, hashing, ct);
            }
            catch (Exception ex)
            {
                return DomainErrors.FileErrors.UploadFailed(ex.Message);
            }

            hash = Convert.ToHexString(hashing.GetHashAndReset()).ToLowerInvariant();
        }

        ErrorOr<UploadedFile> domainResult = UploadedFile.Create(
            entityType: request.EntityType,
            entityId: request.EntityId,
            folder: request.Folder,
            fileName: string.IsNullOrWhiteSpace(originalName) ? storedName : originalName,
            storedFileName: storedName,
            contentType: string.IsNullOrWhiteSpace(request.ContentType) ? "application/octet-stream" : request.ContentType,
            fileExtension: string.IsNullOrWhiteSpace(ext) ? ".bin" : ext,
            fileSize: size,
            contentHash: hash,
            isPrimary: request.IsPrimary);

        if (domainResult.IsError)
        {
         
            await storage.DeleteAsync(storageKey, CancellationToken.None);
            return domainResult.Errors;
        }

        UploadedFile uploaded = domainResult.Value;

        try
        {
            await db.UploadedFiles.AddAsync(uploaded, ct);
            await db.SaveChangesAsync(ct);
        }
        catch
        {
            await storage.DeleteAsync(storageKey, CancellationToken.None);
            throw;
        }

        return new UploadedFileInfo(uploaded.Id, uploaded.ContentHash, uploaded.FileSizeInBytes);
    }

    public async Task<ErrorOr<IReadOnlyList<UploadedFileInfo>>> UploadManyAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFileCollection files,
        CancellationToken ct = default)
    {
        if (files is null || files.Count == 0)
            return DomainErrors.FileErrors.Empty("No files provided.");

        var results = new List<UploadedFileInfo>(files.Count);

        foreach (IFormFile file in files)
        {
            if (file is null || file.Length == 0)
                continue;

            ErrorOr<UploadedFileInfo> single = await UploadAsync(
                entityType, entityId, folder, file, isPrimary: false, ct);

            if (single.IsError)
                return single.Errors;

            results.Add(single.Value);
        }

        if (results.Count == 0)
            return DomainErrors.FileErrors.UploadFailed("All files were empty.");

        return results;
    }

    public async Task<ErrorOr<FileDto>> OpenAsync(Guid id, CancellationToken ct = default)
    {
        UploadedFile? row = await db.UploadedFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        if (row is null)
            return DomainErrors.FileErrors.NotFound();

        Stream? stream = await storage.OpenReadAsync(BuildKey(row.Folder, row.StoredFileName), ct);
        if (stream is null)
            return DomainErrors.FileErrors.NotFound();

        return new FileDto(
            Stream: stream,
            ContentType: row.ContentType,
            FileName: row.FileName,
            ContentHash: row.ContentHash,
            LengthInBytes: row.FileSizeInBytes);
    }

    public async Task<ErrorOr<Success>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        UploadedFile? row = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (row is null)
            return DomainErrors.FileErrors.NotFound();

        db.UploadedFiles.Remove(row);
        await db.SaveChangesAsync(ct);

    
        await storage.DeleteAsync(BuildKey(row.Folder, row.StoredFileName), CancellationToken.None);
        return new Success();
    }

    public async Task<ErrorOr<int>> DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids?.Distinct().ToList() ?? new List<Guid>();
        if (idList.Count == 0)
            return DomainErrors.FileErrors.InvalidType("No file IDs provided.");

        List<UploadedFile> rows = await db.UploadedFiles
            .Where(f => idList.Contains(f.Id))
            .ToListAsync(ct);

        if (rows.Count == 0)
            return DomainErrors.FileErrors.NotFound("No files found for provided IDs.");

        db.UploadedFiles.RemoveRange(rows);
        await db.SaveChangesAsync(ct);

        foreach (UploadedFile row in rows)
        {
            await storage.DeleteAsync(BuildKey(row.Folder, row.StoredFileName), CancellationToken.None);
        }

        return rows.Count;
    }

    private static string BuildKey(string folder, string storedFileName)
        => $"{folder.Trim('/').Replace('\\', '/')}/{storedFileName}";
}
