using Expense_Tracker.Contracts.Responses.Files;

using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;

public record UploadImageCommand(
    string EntityType,
    Guid EntityId,
    string folder,
    IFormFile Image);
