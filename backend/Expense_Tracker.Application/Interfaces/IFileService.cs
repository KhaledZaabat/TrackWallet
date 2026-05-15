using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Interfaces;

/// <summary>
/// Application-facing surface for working with uploaded files. Hides the
/// physical-storage layer behind <see cref="IFileStorage"/> and the database
/// row behind <c>UploadedFile</c>.
/// </summary>
/// <remarks>
/// The service is built around three guarantees:
/// <list type="bullet">
///   <item><b>Atomicity:</b> the on-disk blob and the DB row are both
///   committed, or neither is. A crash mid-upload leaves no orphan row.</item>
///   <item><b>Streaming:</b> uploads and downloads use streams end-to-end.
///   No call buffers the whole file in memory.</item>
///   <item><b>Content-addressable identity:</b> every blob carries a
///   SHA-256 used both as the strong <c>ETag</c> for HTTP caching and as
///   a dedupe key for re-uploaded identical files.</item>
/// </list>
/// </remarks>
public interface IFileService : IScopedService
{
    /// <summary>
    /// Uploads a single file from the inbound HTTP request. Multipart-form
    /// callers should pass the <see cref="IFormFile"/> instance directly
    /// (this is just an adapter to <see cref="UploadAsync(UploadFileRequest, CancellationToken)"/>).
    /// </summary>
    Task<ErrorOr<UploadedFileInfo>> UploadAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFile file,
        bool isPrimary = false,
        CancellationToken ct = default);

    /// <summary>
    /// Generic upload entry point. Used by the HTTP adapter and any internal
    /// caller (e.g. a background ingester).
    /// </summary>
    Task<ErrorOr<UploadedFileInfo>> UploadAsync(
        UploadFileRequest request,
        CancellationToken ct = default);

    /// <summary>
    /// Uploads many files atomically — either all rows are committed and all
    /// blobs persisted, or nothing is.
    /// </summary>
    Task<ErrorOr<IReadOnlyList<UploadedFileInfo>>> UploadManyAsync(
        string entityType,
        Guid entityId,
        string folder,
        IFormFileCollection files,
        CancellationToken ct = default);

    /// <summary>
    /// Opens a streaming read for the given file id.
    /// </summary>
    Task<ErrorOr<FileDto>> OpenAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Deletes the row and the blob. Idempotent: deleting a missing id returns
    /// a NotFound error rather than throwing.
    /// </summary>
    Task<ErrorOr<Success>> DeleteAsync(Guid id, CancellationToken ct = default);

    /// <summary>
    /// Bulk delete for cleanup workflows. Best-effort — partial DB deletes
    /// still reach SaveChanges, and missing blobs are tolerated.
    /// </summary>
    Task<ErrorOr<int>> DeleteManyAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
}
