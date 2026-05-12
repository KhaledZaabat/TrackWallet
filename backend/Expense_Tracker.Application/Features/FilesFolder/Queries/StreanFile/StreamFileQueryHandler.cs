using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using ErrorOr;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.FilesFolder.Queries.StreanFile;

public class StreamFileQueryHandler(IFileService fileService)
{
    public async Task<ErrorOr<StreamFileDto>> Handle(StreamFileQuery request, CancellationToken ct)
   => await fileService.StreamAsync(request.Id, ct);
}
