using ErrorOr;
using Expense_Tracker.Application.Interfaces;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.DeleteFile;

public sealed class DeleteFileCommandHandler(IFileService fileService)
{
    public Task<ErrorOr<Success>> Handle(DeleteFileCommand request, CancellationToken ct)
        => fileService.DeleteAsync(request.Id, ct);
}
