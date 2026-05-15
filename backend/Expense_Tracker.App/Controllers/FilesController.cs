using System.Net.Mime;
using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Commads.DeleteFile;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Features.FilesFolder.Queries.GetFile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Wolverine;

namespace Expense_Tracker.App.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/files")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class FilesController(IMessageBus bus) : ControllerBase
{
    /// <summary>
    /// Inline view of a file, suitable for <c>&lt;img&gt;</c> / <c>&lt;a&gt;</c> rendering.
    /// </summary>
    /// <remarks>
    /// Range requests, <c>If-None-Match</c> / <c>If-Modified-Since</c> short-circuits, and
    /// the <c>ETag</c> header are all owned by ASP.NET's <see cref="ControllerBase.File(System.IO.Stream, string, string?, DateTimeOffset?, EntityTagHeaderValue, bool)"/>
    /// overload. We only contribute the inputs: a strong <see cref="EntityTagHeaderValue"/>
    /// derived from the file's SHA-256 and a <c>Cache-Control</c> header so
    /// browsers reuse the response across page navigations.
    /// </remarks>
    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [EndpointName("GetFile")]
    [EndpointSummary("Returns the file inline (for <img> / <a> rendering).")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> GetFile(Guid id, CancellationToken ct) =>
        ServeAsync(id, asAttachment: false, ct);

    /// <summary>
    /// Force-download with <c>Content-Disposition: attachment</c>. Same caching
    /// and range semantics as <see cref="GetFile"/>.
    /// </summary>
    [HttpGet("{id:guid}/download")]
    [Authorize]
    [EndpointName("DownloadFile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public Task<IActionResult> Download(Guid id, CancellationToken ct) =>
        ServeAsync(id, asAttachment: true, ct);

    /// <summary>
    /// Removes a file (DB row + blob).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize]
    [EndpointName("DeleteFile")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            new DeleteFileCommand(id),
            ct);

        return result.Match<IActionResult>(_ => NoContent(), errs => this.Problem(errs));
    }

    /// <summary>
    /// Loads the file from the application service and hands it to ASP.NET's
    /// <c>File(...)</c> result. The framework owns conditional-GET — we only
    /// supply the validators and cache directives.
    /// </summary>
    private async Task<IActionResult> ServeAsync(Guid id, bool asAttachment, CancellationToken ct)
    {
        ErrorOr<FileDto> result = await bus.InvokeAsync<ErrorOr<FileDto>>(new GetFileQuery(id), ct);
        if (result.IsError)
            return this.Problem(result.Errors);

        FileDto file = result.Value;

        // Strong ETag from the SHA-256 of the bytes. Every row is guaranteed
        // to have a hash because FileService computes it inline during upload.
        var etag = new EntityTagHeaderValue($"\"{file.ContentHash}\"");

        // Cache directive — ASP.NET's File() overload won't set this, so we do.
        Response.Headers.CacheControl = "private, max-age=86400, must-revalidate";

        // For the inline view we still want a Content-Disposition that names
        // the file (drives "Save As..." correctly) without flagging it as an
        // attachment. The attachment path lets the framework set the header
        // from fileDownloadName, which already handles RFC 5987 quoting.
        if (!asAttachment)
        {
            var disposition = new ContentDispositionHeaderValue("inline")
            {
                FileNameStar = file.FileName,
            };
            Response.Headers.ContentDisposition = disposition.ToString();
        }

        return File(
            fileStream: file.Stream,
            contentType: file.ContentType,
            fileDownloadName: asAttachment ? file.FileName : null,
            lastModified: null,
            entityTag: etag,
            enableRangeProcessing: true);
    }
}
