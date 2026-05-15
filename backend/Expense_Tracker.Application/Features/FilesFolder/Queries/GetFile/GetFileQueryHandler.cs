using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Interfaces;

namespace Expense_Tracker.Application.Features.FilesFolder.Queries.GetFile;

public sealed class GetFileQueryHandler(IFileService fileService)
{
    public Task<ErrorOr<FileDto>> Handle(GetFileQuery request, CancellationToken ct)
        => fileService.OpenAsync(request.Id, ct);
}
