using FluentValidation;

namespace Expense_Tracker.Application.Features.Transactions.Commands.CreateTransaction;

public sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.FamilyId)
            .NotEmpty()
            .WithMessage("Family ID is required.");

        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Transaction title is required.")
            .MaximumLength(200)
            .WithMessage("Transaction title cannot exceed 200 characters.");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than zero.")
            .LessThanOrEqualTo(999999999.99m)
            .WithMessage("Amount is too large.");

        RuleFor(x => x.CategoryId)
            .NotEmpty()
            .WithMessage("Category ID is required.");

        RuleFor(x => x.TransactedOn)
            .NotEmpty()
            .WithMessage("Transaction date is required.")
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("Transaction date cannot be in the future.");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid transaction type.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000)
            .WithMessage("Notes cannot exceed 1000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Notes));
    }
}
