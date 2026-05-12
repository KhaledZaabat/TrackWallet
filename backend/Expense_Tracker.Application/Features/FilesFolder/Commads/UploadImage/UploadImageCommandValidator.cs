using Expense_Tracker.Application.Features.FilesFolder.Settings;
using Files.Contracts.Common;
using FluentValidation;

namespace Expense_Tracker.Application.Features.FilesFolder.Commads.UploadImage;

public class UploadImageCommandValidator : AbstractValidator<UploadImageCommand>
{
    public UploadImageCommandValidator()
    {
        RuleFor(x => x.EntityType)
            .NotEmpty()
            .WithMessage("EntityType is required.")
            .MaximumLength(FileSettings.MaxEntityTypeLength)
            .WithMessage($"EntityType cannot exceed {FileSettings.MaxEntityTypeLength} characters.");

        RuleFor(x => x.EntityId)
            .NotEqual(Guid.Empty)
            .WithMessage("EntityId is required.");

        RuleFor(x => x.Image)
            .SetValidator(new FileNotEmptyValidator())
            .SetValidator(new FileSizeValidator())
            .SetValidator(new FileNameValidator())
            .SetValidator(new ImageExtensionValidator())
            .SetValidator(new FileSignatureValidator());
    }
}
