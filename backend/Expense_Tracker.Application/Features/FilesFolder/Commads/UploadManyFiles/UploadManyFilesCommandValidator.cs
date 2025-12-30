using Expense_Tracker.Application.Features.FilesFolder.Commads.UploadManyFiles;
using Expense_Tracker.Application.Features.FilesFolder.Settings;
using Files.Contracts.Common;
using FluentValidation;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadFile;

public class UploadManyFilesCommandValidator : AbstractValidator<UploadManyFilesCommand>
{
    public UploadManyFilesCommandValidator()
    {
        // Validate EntityType
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("EntityType is required.")
            .MaximumLength(FileSettings.MaxEntityTypeLength)
            .WithMessage($"EntityType cannot exceed {FileSettings.MaxEntityTypeLength} characters.");

        // Validate EntityId
        RuleFor(x => x.EntityId)
            .NotEqual(Guid.Empty)
            .WithMessage("EntityId is required.");

        // Validate Files collection
        RuleFor(x => x.Files)
            .NotNull()
            .WithMessage("Files collection is required.")
            .Must(files => files?.Count > 0)
            .WithMessage("At least one file must be provided.");

        // Validate each file in collection
        RuleForEach(x => x.Files)
            .SetValidator(new FileNotEmptyValidator())
            .SetValidator(new FileSizeValidator())
            .SetValidator(new FileNameValidator())
            .SetValidator(new FileSignatureValidator());
    }
}
