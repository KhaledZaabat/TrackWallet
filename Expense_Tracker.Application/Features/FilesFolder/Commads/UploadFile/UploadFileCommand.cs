using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public record UploadFileCommand(
    string folder,
    string EntityType,
    Guid EntityId,
    IFormFile File) : IRequest<Result<UploadFileResponse>>;
