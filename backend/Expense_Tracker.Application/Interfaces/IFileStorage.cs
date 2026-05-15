namespace Expense_Tracker.Application.Interfaces;

/// <summary>
/// Pluggable physical-storage abstraction. The default implementation writes to
/// the local filesystem under <c>AppData/</c>; a future S3 or Azure Blob backend
/// can replace it without touching <see cref="IFileService"/> or any callers.
/// </summary>
public interface IFileStorage : IScopedService
{
    /// <summary>
    /// Persists the supplied stream under <paramref name="key"/>. The write is
    /// atomic — readers either see no file or the fully-written one. The stream
    /// is read from its current position to the end and is not disposed.
    /// </summary>
    /// <returns>Total bytes written.</returns>
    Task<long> SaveAsync(string key, Stream content, CancellationToken ct = default);

    /// <summary>
    /// Opens a read-only stream for <paramref name="key"/> using async I/O. The
    /// caller is responsible for disposing the stream.
    /// </summary>
    /// <returns><c>null</c> when the key does not exist.</returns>
    Task<Stream?> OpenReadAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Removes <paramref name="key"/> from storage. Returns <c>false</c> when
    /// the key does not exist; never throws on "missing".
    /// </summary>
    Task<bool> DeleteAsync(string key, CancellationToken ct = default);

    /// <summary>
    /// Returns <c>true</c> when <paramref name="key"/> exists in storage.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken ct = default);
}
