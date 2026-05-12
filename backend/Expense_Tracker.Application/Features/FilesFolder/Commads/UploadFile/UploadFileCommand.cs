using Expense_Tracker.Contracts.Responses.Files;

using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public record UploadFileCommand(
    string folder,
    string EntityType,
    Guid EntityId,
    IFormFile File);
