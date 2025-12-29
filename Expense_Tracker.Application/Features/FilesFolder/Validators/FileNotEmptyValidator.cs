using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Files.Contracts.Common;

/// <summary>
/// Validates file is not null or empty
/// </summary>
public class FileNotEmptyValidator : AbstractValidator<IFormFile>
{
    public FileNotEmptyValidator()
    {
        RuleFor(x => x)
            .NotNull()
            .WithMessage("File is required.");

        RuleFor(x => x.Length)
            .GreaterThan(0)
            .WithMessage("File cannot be empty.")
            .When(x => x is not null);
    }
}