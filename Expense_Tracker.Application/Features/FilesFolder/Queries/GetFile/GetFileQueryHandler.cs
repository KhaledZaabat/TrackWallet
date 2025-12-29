using Expense_Tracker.Application.Common.Interfaces;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.FilesFolder.Queries.GetFile;

public class GetFileQueryHandler(IFileService fileService)
    : IRequestHandler<GetFileQuery, Result<FileDto>>
{
    public async Task<Result<FileDto>> Handle(GetFileQuery request, CancellationToken ct)

    => await fileService.DownloadAsync(request.Id, ct);




}
