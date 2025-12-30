using Expense_Tracker.Application.Features.FilesFolder.Dtos;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;

namespace Expense_Tracker.Application.Features.FilesFolder.Queries.StreanFile;

public record StreamFileQuery(Guid Id) : IRequest<Result<StreamFileDto>>;
