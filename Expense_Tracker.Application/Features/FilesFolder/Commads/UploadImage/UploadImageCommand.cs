using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;

public record UploadImageCommand(
    string EntityType,
    Guid EntityId,
    string folder,
    IFormFile Image) : IRequest<Result<UploadImageResponse>>;
