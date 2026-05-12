using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;
using ErrorOr;
using Expense_Tracker.Domain.Errors;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;

public class UploadImageCommandHandler(IFileService fileService, [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder)
{
    public async Task<ErrorOr<UploadImageResponse>> Handle(UploadImageCommand request, CancellationToken ct)
    {
        ErrorOr<Guid> uploadResult = await fileService.UploadImageAsync(
            request.EntityType,
            request.EntityId,
            request.folder,
            request.Image,
            false, ct);

        if (uploadResult.IsError)
            return uploadResult.Errors;

        Guid fileId = uploadResult.Value;
        return new UploadImageResponse(fileId, fileUrlBuilder.GetUrl(fileId)!);
    }
}
