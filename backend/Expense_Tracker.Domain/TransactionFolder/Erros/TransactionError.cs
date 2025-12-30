using Expense_Tracker.Domain.Common.ResultPattern.Error;

namespace Expense_Tracker.Domain.TransactionFolder.Erros;

public sealed record TransactionError(DomainErrorCode ApplicationError, string Type, string Description)
       : Error(ApplicationError, Type, Description)
{
    public static readonly TransactionError None =
        new(DomainErrorCode.None, "Transaction.None", string.Empty);

    public static TransactionError BudgetNotEnough(decimal currentBudget, decimal amount) =>
        new(DomainErrorCode.Validation, "Transaction.BudgetNotEnough",
            $"Cannot apply transaction. Expense amount {amount:C} exceeds current budget {currentBudget:C}.");

    public static TransactionError InvalidAmount(decimal amount) =>
        new(DomainErrorCode.Validation, "Transaction.InvalidAmount",
            $"Transaction amount must be greater than zero. Given: {amount:C}");
}