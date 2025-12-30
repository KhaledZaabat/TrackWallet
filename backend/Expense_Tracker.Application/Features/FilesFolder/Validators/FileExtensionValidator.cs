using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Files.Contracts.Common;


/// <summary>
/// Base validator for file extension lists. Use concrete subclasses (e.g. ImageExtensionValidator).
/// </summary>
public abstract class FileExtensionValidator : AbstractValidator<IFormFile>
{
    private readonly string[] _allowedExtensions;

    protected FileExtensionValidator(string[] allowedExtensions)
    {
        _allowedExtensions = allowedExtensions;

        RuleFor(x => x.FileName)
            .Must(fileName =>
            {
                if (string.IsNullOrWhiteSpace(fileName))
                    return false;

                string extension = Path.GetExtension(fileName).ToLowerInvariant();
                return _allowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase);
            })
            .WithMessage($"File extension is not allowed. Allowed extensions: {string.Join(", ", _allowedExtensions)}")
            .When(x => x is not null);
    }
}