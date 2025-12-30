using Expense_Tracker.Application.Features.FilesFolder.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Files.Contracts.Common;

/// <summary>
/// Validates file name contains only safe characters
/// </summary>
public class FileNameValidator : AbstractValidator<IFormFile>
{
    public FileNameValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required.")
            .MaximumLength(FileSettings.MaxFileNameLength)
            .WithMessage($"File name cannot exceed {FileSettings.MaxFileNameLength} characters.")

            .Matches(@"^[^\\/:*?""<>|]+$")
            .WithMessage("File name contains invalid characters.")
            .When(x => x is not null);
    }
}

