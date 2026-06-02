using Expense_Tracker.Application.Constans;
using Files.Contracts.Common;
using FluentValidation;

namespace Expense_Tracker.Application.Features.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        // Email
        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage(ValidationMessages.EmailRequired)
            .Matches(ValidationPatterns.Email)
            .WithMessage(ValidationMessages.InvalidEmail)
            .MaximumLength(ValidationLimits.EmailMaxLength)
            .WithMessage($"Email cannot exceed {ValidationLimits.EmailMaxLength} characters.");

        // Password
        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage(ValidationMessages.PasswordRequired)
            .MinimumLength(ValidationLimits.PasswordMinLength)
            .WithMessage(ValidationMessages.PasswordTooShort)
            .Matches(ValidationPatterns.StrongPassword)
            .WithMessage(ValidationMessages.WeakPassword)
            .MaximumLength(ValidationLimits.PasswordMaxLength)
            .WithMessage(
                $"Password cannot exceed {ValidationLimits.PasswordMaxLength} characters."
            );

        // UserName — start with a letter, then 2-19 letters/digits/underscores.
        // Mirrors the SPA regex /^[a-zA-Z][a-zA-Z0-9_]{2,19}$/.
        RuleFor(x => x.UserName)
            .NotEmpty()
            .WithMessage(ValidationMessages.UserNameRequired)
            .MinimumLength(ValidationLimits.UserNameMinLength)
            .WithMessage(ValidationMessages.InvalidUserName)
            .MaximumLength(ValidationLimits.UserNameMaxLength)
            .WithMessage(ValidationMessages.InvalidUserName)
            .Matches(ValidationPatterns.UserName)
            .WithMessage(ValidationMessages.InvalidUserName);

        // FullName
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required)
            .MaximumLength(ValidationLimits.NameMaxLength)
            .WithMessage($"Full name cannot exceed {ValidationLimits.NameMaxLength} characters.");

        // BirthDate
        RuleFor(x => x.BirthDate)
            .NotEmpty()
            .WithMessage(ValidationMessages.Required)
            .LessThan(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Birth date must be in the past.");

        RuleFor(x => x.ProfileImage)
            .SetValidator(new FileNotEmptyValidator()!)
            .SetValidator(new FileSizeValidator()!) // 5MB
            .SetValidator(new FileNameValidator()!)
            .SetValidator(new ImageExtensionValidator()!)
            .SetValidator(new FileSignatureValidator()!)
            .When(x => x.ProfileImage is not null);
    }
}
