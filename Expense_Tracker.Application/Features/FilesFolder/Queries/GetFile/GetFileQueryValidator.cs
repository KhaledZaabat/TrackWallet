using FluentValidation;

namespace Expense_Tracker.Application.Features.FilesFolder.Queries.GetFile;

public class GetFileQueryValidator : AbstractValidator<GetFileQuery>
{
    public GetFileQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("File ID is required.");
    }
}
