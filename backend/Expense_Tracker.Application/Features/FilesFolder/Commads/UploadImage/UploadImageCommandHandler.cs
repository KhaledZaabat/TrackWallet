using ErrorOr;
using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;

public sealed class UploadImageCommandHandler(
    IFileService fileService,
    IFileUrlResolver fileUrlResolver)
{
    public async Task<ErrorOr<UploadImageResponse>> Handle(UploadImageCommand request, CancellationToken ct)
    {
        ErrorOr<Application.Features.FilesFolder.Dtos.UploadedFileInfo> result =
            await fileService.UploadAsync(
                request.EntityType,
                request.EntityId,
                request.folder,
                request.Image,
                isPrimary: false,
                ct);

        if (result.IsError)
            return result.Errors;

        return new UploadImageResponse(result.Value.FileId, fileUrlResolver.GetUrl(result.Value.FileId)!);
    }
}
