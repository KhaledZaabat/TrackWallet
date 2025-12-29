using FluentValidation;

namespace Expense_Tracker.Application.Features.FilesFolder.Queries.StreanFile;

public class StreamFileQueryValidator : AbstractValidator<StreamFileQuery>
{
    public StreamFileQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEqual(Guid.Empty)
            .WithMessage("File ID is required.");
    }
}
