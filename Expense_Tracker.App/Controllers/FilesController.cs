using Asp.Versioning;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Features.FilesFolder.Queries.GetFile;
using Expense_Tracker.Application.Features.FilesFolder.Queries.StreanFile;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Expense_Tracker.App.Controllers;

[Route("api/[controller]")]
[ApiVersion("1.0")]

[ApiController]
public class FilesController(ISender sender) : ControllerBase
{
    //[HttpPost("upload")]
    //[RequestSizeLimit(200_000_000)]
    //public async Task<ActionResult<UploadFileResponse>> Upload(
    //    [FromForm] UploadFileRequest request,
    //    CancellationToken ct)
    //{
    //    var command = new UploadFileCommand(
    //        request.EntityType,
    //        request.EntityId,
    //        request.File);

    //    Result<UploadFileResponse> result = await sender.Send(command, ct);

    //    if (result.IsFailure)
    //        return BadRequest(new { Error = result.TryGetError().Description });

    //    var response = result.TryGetValue();
    //    return CreatedAtAction(
    //        nameof(GetFile),
    //        new { id = response.FileId },
    //        response);
    //}

    //[HttpPost("upload-many")]
    //[RequestSizeLimit(500_000_000)]
    //public async Task<ActionResult<UploadManyFilesResponse>> UploadMany(
    //    [FromForm] UploadManyFilesRequest request,
    //    CancellationToken ct)
    //{
    //    var command = new UploadManyFilesCommand(
    //        request.EntityType,
    //        request.EntityId,
    //        request.Files);

    //    var result = await sender.Send(command, ct);

    //    return result.ToActionResult<UploadManyFilesResponse>(HttpContext);
    //}

    //[HttpPost("upload-image")]
    //[RequestSizeLimit(20_000_000)]
    //public async Task<ActionResult<UploadImageResponse>> UploadImage(
    //    [FromForm] UploadImageRequest request,
    //    CancellationToken ct)
    //{
    //    var command = new UploadImageCommand(
    //        request.EntityType,
    //        request.EntityId,
    //        request.Image);

    //    var result = await sender.Send(command, ct);

    //    if (result.IsFailure)
    //        return BadRequest(new { Error = result.TryGetError().Description });

    //    var response = result.TryGetValue();
    //    var imageUrl = Url.ActionLink(nameof(GetFile), "Files", new { id = response.FileId })!;

    //    return CreatedAtAction(
    //        nameof(GetFile),
    //        new { id = response.FileId },
    //        new UploadImageResponse(response.FileId, imageUrl));
    //}

    //[HttpPost("upload-images")]
    //[RequestSizeLimit(100_000_000)]
    //public async Task<ActionResult<UploadImagesResponse>> UploadImages(
    //    [FromForm] UploadImagesRequest request,
    //    CancellationToken ct)
    //{
    //    var command = new UploadImagesCommand(
    //        request.EntityType,
    //        request.EntityId,
    //        request.Images);

    //    var result = await sender.Send(command, ct);

    //    if (result.IsFailure)
    //        return BadRequest(new { Error = result.TryGetError().Description });

    //    var imageIds = result.TryGetValue().ToList();
    //    var imageUrls = imageIds.Select(id =>
    //        Url.ActionLink(nameof(GetFile), "Files", new { id })!).ToList();

    //    return Ok(new UploadImagesResponse(imageIds.Count, imageIds, imageUrls));
    //}

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
        Result<FileDto> result = await sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { Error = result.TryGetError().Description });

        FileDto file = result.TryGetValue();

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
        var result = await sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { Error = result.TryGetError().Description });

        var file = result.TryGetValue();
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
        var result = await sender.Send(query, ct);

        if (result.IsFailure)
            return NotFound(new { Error = result.TryGetError().Description });

        var file = result.TryGetValue();
        return File(file.Stream!, file.ContentType, enableRangeProcessing: true);
    }

}
