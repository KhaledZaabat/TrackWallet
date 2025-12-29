using Expense_Tracker.Application.Common.Interfaces;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.FilesFolder.Queries.StreanFile;

public class StreamFileQueryHandler(IFileService fileService)
    : IRequestHandler<StreamFileQuery, Result<StreamFileDto>>
{
    public async Task<Result<StreamFileDto>> Handle(StreamFileQuery request, CancellationToken ct)
   => await fileService.StreamAsync(request.Id, ct);


}