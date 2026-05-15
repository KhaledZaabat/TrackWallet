using ErrorOr;
using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Application.Interfaces;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImages;

public sealed class UploadImagesCommandHandler(IFileService fileService)
{
    public async Task<ErrorOr<IEnumerable<Guid>>> Handle(UploadImagesCommand request, CancellationToken ct)
    {
        ErrorOr<IReadOnlyList<UploadedFileInfo>> result = await fileService.UploadManyAsync(
            request.EntityType,
            request.EntityId,
            request.folder,
            request.Images,
            ct);

        if (result.IsError)
            return result.Errors;

        IEnumerable<Guid> ids = result.Value.Select(x => x.FileId).ToList();
        return ErrorOrFactory.From(ids);
    }
}
