using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using ErrorOr;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.FilesFolder.Queries.GetFile;

public class GetFileQueryHandler(IFileService fileService)
{
    public async Task<ErrorOr<FileDto>> Handle(GetFileQuery request, CancellationToken ct)

    => await fileService.DownloadAsync(request.Id, ct);
}
