using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using Microsoft.AspNetCore.Http;

public interface IFileService
{
    /// <summary>
    /// Upload a single file to a specific entity and folder.
    /// </summary>
    Task<Result<Guid>> UploadAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile file,
        CancellationToken ct = default);

    /// <summary>
    /// Upload multiple files to a specific entity and folder.
    /// </summary>
    Task<Result<IEnumerable<Guid>>> UploadManyAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFileCollection files,
        CancellationToken ct = default);

    /// <summary>
    /// Upload a single image with optional primary flag.
    /// </summary>
    Task<Result<Guid>> UploadImageAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile image,

        bool primary = false,

        CancellationToken ct = default);

    /// <summary>
    /// Download a file as a byte array DTO.
    /// </summary>
    Task<Result<FileDto>> DownloadAsync(
        Guid id,
        CancellationToken ct = default);

    /// <summary>
    /// Stream a file directly.
    /// </summary>
    Task<Result<StreamFileDto>> StreamAsync(
        Guid id,
        CancellationToken ct = default);

    /// <summary>
    /// Delete a file physically and from database.
    /// </summary>
    Task<Result> DeleteAsync(
        Guid id,
        CancellationToken ct = default);


    public Task<Result> DeleteManyAsync(
    IEnumerable<Guid> ids,
    CancellationToken ct = default);
}

