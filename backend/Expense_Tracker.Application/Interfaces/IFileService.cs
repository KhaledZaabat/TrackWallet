using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Microsoft.AspNetCore.Http;

public interface IFileService
{
    Task<ErrorOr<Guid>> UploadAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile file,
        CancellationToken ct = default);

    Task<ErrorOr<IEnumerable<Guid>>> UploadManyAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFileCollection files,
        CancellationToken ct = default);

    Task<ErrorOr<Guid>> UploadImageAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile image,
        bool primary = false,
        CancellationToken ct = default);

    Task<ErrorOr<FileDto>> DownloadAsync(
        Guid id,
        CancellationToken ct = default);

    Task<ErrorOr<StreamFileDto>> StreamAsync(
        Guid id,
        CancellationToken ct = default);

    Task<ErrorOr<Success>> DeleteAsync(
        Guid id,
        CancellationToken ct = default);

    Task<ErrorOr<Success>> DeleteManyAsync(
        IEnumerable<Guid> ids,
        CancellationToken ct = default);
}
