using Expense_Tracker.Application.Features.FilesFolder.Commads.UploadManyFiles;
using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public class UploadManyFilesCommandHandler(IFileService fileService)
    : IRequestHandler<UploadManyFilesCommand, Result<UploadManyFilesResponse>>
{
    public async Task<Result<UploadManyFilesResponse>> Handle(UploadManyFilesCommand request, CancellationToken ct)
    {
        var uploadResult = await fileService.UploadManyAsync(

             request.EntityType,
             request.EntityId,
             request.folder,
             request.Files,
             ct);
        if (uploadResult.IsFailure)
            return Result.Failure<UploadManyFilesResponse>(uploadResult.TryGetError());
        IEnumerable<Guid> value = uploadResult.TryGetValue();

        return Result.Success(new UploadManyFilesResponse(value.Count(), value.ToList()));

    }
}
