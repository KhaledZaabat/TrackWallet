using ErrorOr;
using Expense_Tracker.Domain.Errors;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImages;

public class UploadImagesCommandHandler(IFileService fileService)
{
    public async Task<ErrorOr<IEnumerable<Guid>>> Handle(UploadImagesCommand request, CancellationToken ct)
    {
        return await fileService.UploadManyAsync(
            request.EntityType,
            request.EntityId,
            request.folder,
            request.Images,
             ct);
    }
}
