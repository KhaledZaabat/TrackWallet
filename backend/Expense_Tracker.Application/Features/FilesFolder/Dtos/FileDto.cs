namespace Expense_Tracker.Application.Features.FilesFolder.Dtos;

/// <summary>
/// Streaming download payload — the controller's <see cref="Microsoft.AspNetCore.Mvc.FileStreamResult"/>
/// will dispose <see cref="Stream"/> after sending it to the client.
/// </summary>
/// <param name="Stream">Open read-only stream positioned at the start of the blob.</param>
/// <param name="ContentType">MIME type as recorded at upload time.</param>
/// <param name="FileName">Original (display) file name as the user supplied it.</param>
/// <param name="ContentHash">Lower-case hex SHA-256, used as the strong ETag.</param>
/// <param name="LengthInBytes">Total length of the stream (so the response can advertise <c>Content-Length</c>).</param>
public sealed record FileDto(
    Stream Stream,
    string ContentType,
    string FileName,
    string ContentHash,
    long LengthInBytes);
