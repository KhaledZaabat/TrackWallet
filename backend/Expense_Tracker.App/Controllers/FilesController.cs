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
