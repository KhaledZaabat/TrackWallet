
using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public class UploadFileCommandHandler(IFileService fileService)
    : IRequestHandler<UploadFileCommand, Result<UploadFileResponse>>
{
    public async Task<Result<UploadFileResponse>> Handle(UploadFileCommand request, CancellationToken ct)
    {
        Result<Guid> uploadResult = await fileService.UploadAsync(
             request.EntityType,
             request.EntityId,
             request.folder,
             request.File,
             ct);
        if (uploadResult.IsFailure)
            return Result.Failure<UploadFileResponse>(uploadResult.TryGetError());

        return Result.Success(new UploadFileResponse(uploadResult.TryGetValue()));


    }
}
