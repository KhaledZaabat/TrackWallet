using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Expense_Tracker.Domain.Files;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Services;

public class FileService(IAppDbContext db, IWebHostEnvironment env) : IFileService, IScopedService
{
    private readonly string _rootPath = Path.Combine(env.ContentRootPath, "AppData");

    // ---------------------------------------------------------
    //  Upload a single file (not primary)
    // ---------------------------------------------------------
    public async Task<Result<Guid>> UploadAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile file,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return Result.Failure<Guid>(FileError.EmptyFile());

        var uploadedResult = await Save(file, entityType, entityId, folder, isPrimary: false, ct);

        if (uploadedResult.IsFailure)
            return Result.Failure<Guid>(uploadedResult.TryGetError());

        var uploaded = uploadedResult.TryGetValue();

        await db.UploadedFiles.AddAsync(uploaded, ct);
        await db.SaveChangesAsync(ct);

        return Result.Success(uploaded.Id);
    }

    // ---------------------------------------------------------
    //  Upload many files (none are primary)
    // ---------------------------------------------------------
    public async Task<Result<IEnumerable<Guid>>> UploadManyAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFileCollection files,
        CancellationToken ct = default)
    {
        var list = new List<UploadedFile>();

        foreach (var file in files)
        {
            if (file is null || file.Length == 0)
                continue;

            var saved = await Save(file, entityType, entityId, folder, isPrimary: false, ct);

            if (saved.IsFailure)
                return Result.Failure<IEnumerable<Guid>>(saved.TryGetError());

            list.Add(saved.TryGetValue());
        }

        if (list.Count == 0)
            return Result.Failure<IEnumerable<Guid>>(FileError.UploadFailed("All files are invalid."));

        await db.UploadedFiles.AddRangeAsync(list, ct);
        await db.SaveChangesAsync(ct);

        return Result.Success(list.Select(f => f.Id));
    }

    // ---------------------------------------------------------
    //  Upload an IMAGE (supports primary flag)
    // ---------------------------------------------------------
    public async Task<Result<Guid>> UploadImageAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile image,
        bool isPrimary = false,
        CancellationToken ct = default)
    {
        if (image is null || image.Length == 0)
            return Result.Failure<Guid>(FileError.EmptyFile());

        var saved = await Save(image, entityType, entityId, folder, isPrimary, ct);

        if (saved.IsFailure)
            return Result.Failure<Guid>(saved.TryGetError());

        var file = saved.TryGetValue();

        await db.UploadedFiles.AddAsync(file, ct);
        await db.SaveChangesAsync(ct);

        return Result.Success(file.Id);
    }

    // ---------------------------------------------------------
    //  Download file
    // ---------------------------------------------------------
    public async Task<Result<FileDto>> DownloadAsync(Guid id, CancellationToken ct = default)
    {
        var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null)
            return Result.Failure<FileDto>(FileError.FileNotFound());

        var path = Path.Combine(_rootPath, file.Folder, file.StoredFileName);

        if (!System.IO.File.Exists(path))
            return Result.Failure<FileDto>(FileError.FileNotFound());

        var bytes = await System.IO.File.ReadAllBytesAsync(path, ct);

        return Result.Success(new FileDto(bytes, file.ContentType, file.FileName));
    }

    // ---------------------------------------------------------
    //  Stream file
    // ---------------------------------------------------------
    public async Task<Result<StreamFileDto>> StreamAsync(Guid id, CancellationToken ct = default)
    {
        var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null)
            return Result.Failure<StreamFileDto>(FileError.FileNotFound());

        var path = Path.Combine(_rootPath, file.Folder, file.StoredFileName);

        if (!System.IO.File.Exists(path))
            return Result.Failure<StreamFileDto>(FileError.FileNotFound());

        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            useAsync: true);

        return Result.Success(new StreamFileDto(stream, file.ContentType, file.FileName));
    }

    // ---------------------------------------------------------
    //  Delete file
    // ---------------------------------------------------------
    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null)
            return Result.Failure(FileError.FileNotFound());

        string path = Path.Combine(_rootPath, file.Folder, file.StoredFileName);

        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

        db.UploadedFiles.Remove(file);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ---------------------------------------------------------
    //  Delete many files
    // ---------------------------------------------------------
    public async Task<Result> DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        if (ids is null || !ids.Any())
            return Result.Failure(FileError.InvalidFileType("No file IDs provided."));

        var files = await db.UploadedFiles
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(ct);

        if (files.Count == 0)
            return Result.Failure(FileError.FileNotFound("No files found for provided IDs."));

        foreach (var file in files)
        {
            string path = Path.Combine(_rootPath, file.Folder, file.StoredFileName);

            if (System.IO.File.Exists(path))
            {
                try { System.IO.File.Delete(path); }
                catch { /* ignore */ }
            }
        }

        db.UploadedFiles.RemoveRange(files);
        await db.SaveChangesAsync(ct);

        return Result.Success();
    }

    // ---------------------------------------------------------
    //  Core SAVE method 
    // ---------------------------------------------------------
    private async Task<Result<UploadedFile>> Save(
        IFormFile file,
        string entityType,
        Guid entityId,
        string folder,
        bool isPrimary,
        CancellationToken ct)
    {
        Directory.CreateDirectory(Path.Combine(_rootPath, folder));

        string original = Path.GetFileName(file.FileName);
        string ext = Path.GetExtension(original).ToLowerInvariant();
        string stored = $"{Guid.NewGuid():N}{ext}";

        string path = Path.Combine(_rootPath, folder, stored);

        using (var stream = new FileStream(path, FileMode.Create))
            await file.CopyToAsync(stream, ct);

        return UploadedFile.Create(
            entityType,
            entityId,
            folder,
            original,
            stored,
            file.ContentType,
            ext,
            file.Length,
            isPrimary
         );
    }
}
