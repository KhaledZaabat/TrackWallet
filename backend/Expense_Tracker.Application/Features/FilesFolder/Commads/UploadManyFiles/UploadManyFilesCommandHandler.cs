using Expense_Tracker.Application.Features.FilesFolder.Commads.UploadManyFiles;
using Expense_Tracker.Contracts.Responses.Files;
using ErrorOr;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public class UploadManyFilesCommandHandler(IFileService fileService)
{
    public async Task<ErrorOr<UploadManyFilesResponse>> Handle(UploadManyFilesCommand request, CancellationToken ct)
    {
        var uploadResult = await fileService.UploadManyAsync(

             request.EntityType,
             request.EntityId,
             request.folder,
             request.Files,
             ct);

        if (uploadResult.IsError)
            return uploadResult.Errors;

        IEnumerable<Guid> value = uploadResult.Value;

        return new UploadManyFilesResponse(value.Count(), value.ToList());
    }
}
