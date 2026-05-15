using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Commads.UploadManyFiles;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public sealed class UploadManyFilesCommandHandler(IFileService fileService)
{
    public async Task<ErrorOr<UploadManyFilesResponse>> Handle(UploadManyFilesCommand request, CancellationToken ct)
    {
        ErrorOr<IReadOnlyList<UploadedFileInfo>> result = await fileService.UploadManyAsync(
            request.EntityType,
            request.EntityId,
            request.folder,
            request.Files,
            ct);

        if (result.IsError)
            return result.Errors;

        List<Guid> ids = result.Value.Select(x => x.FileId).ToList();
        return new UploadManyFilesResponse(ids.Count, ids);
    }
}
