using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Features.FilesFolder.Queries.GetFile;
using Expense_Tracker.Application.Features.FilesFolder.Queries.StreanFile;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[Route("api/[controller]")]
[ApiVersion("1.0")]
[ApiController]
public class FilesController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Retrieves a file by its ID and returns it inline (not downloaded),
    /// typically used for images rendered in the UI.
    /// </summary>
    /// <param name="id">The ID of the file to retrieve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Returns the file content in the response body, with the correct content type.
    /// </returns>
    /// <response code="200">File retrieved successfully.</response>
    /// <response code="404">File not found.</response>
    [HttpGet("{id}")]
    public async Task<ActionResult> GetFile(Guid id, CancellationToken ct)
    {
        GetFileQuery query = new GetFileQuery(id);
        ErrorOr<FileDto> result = await bus.InvokeAsync<ErrorOr<FileDto>>(query, ct);

        if (result.IsError)
            return NotFound(new { Error = result.Errors.First().Description });

        FileDto file = result.Value;

        // Show the file in browser instead of downloading
        Response.Headers.Append("Content-Disposition", $"inline; filename=\"{file.FileName}\"");

        return File(file.Content, file.ContentType);
    }

    /// <summary>
    /// Downloads a file by its ID, enabling the browser to open a download dialog.
    /// Supports range processing for large files.
    /// </summary>
    /// <param name="id">The ID of the file to download.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Returns the file as a downloadable attachment.
    /// </returns>
    /// <response code="200">File downloaded successfully.</response>
    /// <response code="404">File not found.</response>
    [HttpGet("{id}/download")]
    public async Task<ActionResult> Download(Guid id, CancellationToken ct)
    {
        var query = new GetFileQuery(id);
        var result = await bus.InvokeAsync<ErrorOr<FileDto>>(query, ct);

        if (result.IsError)
            return NotFound(new { Error = result.Errors.First().Description });

        var file = result.Value;
        return File(file.Content, file.ContentType, file.FileName, enableRangeProcessing: true);
    }

    /// <summary>
    /// Streams a file efficiently using a <see cref="Stream"/> without loading it fully into memory.
    /// Recommended for large video/audio files or high-resolution media.
    /// Supports HTTP range requests (resume, seek).
    /// </summary>
    /// <param name="id">The ID of the file to stream.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// A streamed file response with range processing enabled.
    /// </returns>
    /// <response code="200">File stream started successfully.</response>
    /// <response code="404">File not found.</response>
    [HttpGet("{id:guid}/stream")]
    public async Task<ActionResult> Stream(Guid id, CancellationToken ct)
    {
        var query = new StreamFileQuery(id);
        var result = await bus.InvokeAsync<ErrorOr<StreamFileDto>>(query, ct);

        if (result.IsError)
            return NotFound(new { Error = result.Errors.First().Description });

        var file = result.Value;
        return File(file.Stream!, file.ContentType, enableRangeProcessing: true);
    }
}
