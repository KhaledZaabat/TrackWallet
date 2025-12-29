using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImages;

public class UploadImagesCommandHandler(IFileService fileService)
    : IRequestHandler<UploadImagesCommand, Result<IEnumerable<Guid>>>
{
    public async Task<Result<IEnumerable<Guid>>> Handle(UploadImagesCommand request, CancellationToken ct)
    {
        return await fileService.UploadManyAsync(
            request.EntityType,
            request.EntityId,
            request.folder,
            request.Images,
             ct);
    }
}