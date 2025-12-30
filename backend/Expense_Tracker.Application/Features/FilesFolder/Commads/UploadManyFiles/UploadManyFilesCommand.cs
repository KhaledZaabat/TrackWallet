using Expense_Tracker.Contracts.Responses.Files;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadManyFiles;

public record UploadManyFilesCommand(
    string folder,
    string EntityType,
    Guid EntityId,
    IFormFileCollection Files) : IRequest<Result<UploadManyFilesResponse>>;
