using Expense_Tracker.Contracts.Responses.Files;
using ErrorOr;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public class UploadFileCommandHandler(IFileService fileService)
{
    public async Task<ErrorOr<UploadFileResponse>> Handle(UploadFileCommand request, CancellationToken ct)
    {
        ErrorOr<Guid> uploadResult = await fileService.UploadAsync(
             request.EntityType,
             request.EntityId,
             request.folder,
             request.File,
             ct);

        if (uploadResult.IsError)
            return uploadResult.Errors;

        return new UploadFileResponse(uploadResult.Value);
    }
}
