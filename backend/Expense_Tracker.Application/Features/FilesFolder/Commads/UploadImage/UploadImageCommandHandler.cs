using Expense_Tracker.Application.Interfaces;
using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;

public class UploadImageCommandHandler(IFileService fileService, [FromKeyedServices("files")] IUrlBuilder fileUrlBuilder)
    : IRequestHandler<UploadImageCommand, Result<UploadImageResponse>>
{
    public async Task<Result<UploadImageResponse>> Handle(UploadImageCommand request, CancellationToken ct)
    {
        Result<Guid> uploadResult = await fileService.UploadImageAsync(
            request.EntityType,
            request.EntityId,
            request.folder,
            request.Image,
            false, ct);
        if (uploadResult.IsFailure)
            return Result.Failure<UploadImageResponse>(uploadResult.TryGetError());

        Guid fileId = uploadResult.TryGetValue();
        return Result.Success(new UploadImageResponse(fileId, fileUrlBuilder.GetUrl(fileId)!));
    }
}