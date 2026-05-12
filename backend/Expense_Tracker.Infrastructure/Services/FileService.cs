using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Domain.Errors;
using Expense_Tracker.Domain.Files;
using Expense_Tracker.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Expense_Tracker.Infrastructure.Services;

public class FileService(AppDbContext db, IWebHostEnvironment env) : IFileService, IScopedService
{
    private readonly string _rootPath = Path.Combine(env.ContentRootPath, "AppData");

    
    public async Task<ErrorOr<Guid>> UploadAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile file,
        CancellationToken ct = default)
    {
        if (file is null || file.Length == 0)
            return DomainErrors.FileErrors.Empty();

        var uploadedResult = await Save(file, entityType, entityId, folder, isPrimary: false, ct);

        if (uploadedResult.IsError)
            return uploadedResult.Errors;

        var uploaded = uploadedResult.Value;

        await db.UploadedFiles.AddAsync(uploaded, ct);
        await db.SaveChangesAsync(ct);

        return uploaded.Id;
    }

   
    public async Task<ErrorOr<IEnumerable<Guid>>> UploadManyAsync(
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

            if (saved.IsError)
                return saved.Errors;

            list.Add(saved.Value);
        }

        if (list.Count == 0)
            return DomainErrors.FileErrors.UploadFailed("All files are invalid.");

        await db.UploadedFiles.AddRangeAsync(list, ct);
        await db.SaveChangesAsync(ct);

        return list.Select(f => f.Id).ToList();
    }

    // ---------------------------------------------------------
    //  Upload an IMAGE (supports primary flag)
    // ---------------------------------------------------------
    public async Task<ErrorOr<Guid>> UploadImageAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile image,
        bool isPrimary = false,
        CancellationToken ct = default)
    {
        if (image is null || image.Length == 0)
            return DomainErrors.FileErrors.Empty();

        var saved = await Save(image, entityType, entityId, folder, isPrimary, ct);

        if (saved.IsError)
            return saved.Errors;

        var file = saved.Value;

        await db.UploadedFiles.AddAsync(file, ct);
        await db.SaveChangesAsync(ct);

        return file.Id;
    }

    // ---------------------------------------------------------
    //  Download file
    // ---------------------------------------------------------
    public async Task<ErrorOr<FileDto>> DownloadAsync(Guid id, CancellationToken ct = default)
    {
        var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null)
            return DomainErrors.FileErrors.NotFound();

        var path = Path.Combine(_rootPath, file.Folder, file.StoredFileName);

        if (!System.IO.File.Exists(path))
            return DomainErrors.FileErrors.NotFound();

        var bytes = await System.IO.File.ReadAllBytesAsync(path, ct);

        return new FileDto(bytes, file.ContentType, file.FileName);
    }

    // ---------------------------------------------------------
    //  Stream file
    // ---------------------------------------------------------
    public async Task<ErrorOr<StreamFileDto>> StreamAsync(Guid id, CancellationToken ct = default)
    {
        var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null)
            return DomainErrors.FileErrors.NotFound();

        var path = Path.Combine(_rootPath, file.Folder, file.StoredFileName);

        if (!System.IO.File.Exists(path))
            return DomainErrors.FileErrors.NotFound();

        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            64 * 1024,
            useAsync: true);

        return new StreamFileDto(stream, file.ContentType, file.FileName);
    }

    // ---------------------------------------------------------
    //  Delete file
    // ---------------------------------------------------------
    public async Task<ErrorOr<Success>> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var file = await db.UploadedFiles.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (file is null)
            return DomainErrors.FileErrors.NotFound();

        string path = Path.Combine(_rootPath, file.Folder, file.StoredFileName);

        if (System.IO.File.Exists(path))
            System.IO.File.Delete(path);

        db.UploadedFiles.Remove(file);
        await db.SaveChangesAsync(ct);

        return new Success();
    }

    // ---------------------------------------------------------
    //  Delete many files
    // ---------------------------------------------------------
    public async Task<ErrorOr<Success>> DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        if (ids is null || !ids.Any())
            return DomainErrors.FileErrors.InvalidType("No file IDs provided.");

        var files = await db.UploadedFiles
            .Where(f => ids.Contains(f.Id))
            .ToListAsync(ct);

        if (files.Count == 0)
            return DomainErrors.FileErrors.NotFound("No files found for provided IDs.");

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

        return new Success();
    }

    // ---------------------------------------------------------
    //  Core SAVE method 
    // ---------------------------------------------------------
    private async Task<ErrorOr<UploadedFile>> Save(
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
