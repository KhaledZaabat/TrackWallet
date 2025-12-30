using Expense_Tracker.Application.Features.FilesFolder.Settings;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Files.Contracts.Common;

public class FileSizeValidator : AbstractValidator<IFormFile>
{
    public FileSizeValidator()
    {
        RuleFor(x => x.Length)
            .LessThanOrEqualTo(FileSettings.MaxFileSizeInBytes)
            .WithMessage($"Max file size is {FileSettings.MaxFileSizeInMB} MB.")
            .When(x => x is not null);
    }
}