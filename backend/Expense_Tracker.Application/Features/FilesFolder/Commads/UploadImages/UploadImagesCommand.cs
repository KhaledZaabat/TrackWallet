using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImages;

public record UploadImagesCommand(
    string folder,
    string EntityType,
    Guid EntityId,
    IFormFileCollection Images) : IRequest<Result<IEnumerable<Guid>>>;
