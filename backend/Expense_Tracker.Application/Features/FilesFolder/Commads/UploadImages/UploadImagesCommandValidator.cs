using Expense_Tracker.Application.Features.FilesFolder.Settings;
using Files.Contracts.Common;
using FluentValidation;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImages;

public class UploadImagesCommandValidator : AbstractValidator<UploadImagesCommand>
{
    public UploadImagesCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("EntityType is required.")
            .MaximumLength(FileSettings.MaxEntityTypeLength)
            .WithMessage($"EntityType cannot exceed {FileSettings.MaxEntityTypeLength} characters.");

        RuleFor(x => x.EntityId)
            .NotEqual(Guid.Empty)
            .WithMessage("EntityId is required.");

        RuleFor(x => x.Images)
            .NotNull()
            .WithMessage("Images collection is required.")
            .Must(images => images?.Count > 0)
            .WithMessage("At least one image must be provided.");

        RuleForEach(x => x.Images)
            .SetValidator(new FileNotEmptyValidator())
            .SetValidator(new FileSizeValidator())
            .SetValidator(new FileNameValidator())
            .SetValidator(new ImageExtensionValidator())
            .SetValidator(new FileSignatureValidator());
    }
}
