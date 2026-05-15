using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public sealed class UploadFileCommandHandler(IFileService fileService)
{
    public async Task<ErrorOr<UploadFileResponse>> Handle(UploadFileCommand request, CancellationToken ct)
    {
        var result = await fileService.UploadAsync(
            request.EntityType,
            request.EntityId,
            request.folder,
            request.File,
            isPrimary: false,
            ct);

        if (result.IsError)
            return result.Errors;

        return new UploadFileResponse(result.Value.FileId);
    }
}
