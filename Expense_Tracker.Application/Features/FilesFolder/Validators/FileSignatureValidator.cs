using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Files.Contracts.Common;

/// <summary>
/// Validates file signature against allowed and blocked signatures
/// </summary>
public class FileSignatureValidator : AbstractValidator<IFormFile>
{
    public FileSignatureValidator()
    {
        RuleFor(x => x)
            .MustAsync(async (file, ct) =>
            {
                if (file == null || file.Length == 0)
                    return false;

                return await FileHelper.ValidateFileSignatureAsync(file, ct);
            })
            .WithMessage("Invalid or blocked file signature.")
            .When(x => x is not null);
    }
}