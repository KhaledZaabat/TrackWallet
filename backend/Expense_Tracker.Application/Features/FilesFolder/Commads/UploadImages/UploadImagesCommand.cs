using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImages;

public record UploadImagesCommand(
    string folder,
    string EntityType,
    Guid EntityId,
    IFormFileCollection Images);
