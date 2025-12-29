using FluentValidation;
using MediatR;
using Expense_Tracker.Application.Common.Errors;
using Expense_Tracker.Domain.Common.ResultPattern.Result;

namespace Expense_Tracker.Application.Common.Behaivors;

public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);
        var validations = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validations
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .Select(f => f.ErrorMessage)
            .ToArray();

        if (failures.Length > 0)
        {
            string message = string.Join(Environment.NewLine, failures);

            // Create Application Validation Error
            var error = ValidationError.InvalidInput(message);

            // Check if TResponse is Result<T>
            if (typeof(TResponse).IsGenericType &&
                typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var innerType = typeof(TResponse).GetGenericArguments()[0];

                // Create Result<T>.Failure<T>(error)
                var failureMethod = typeof(Result)
                    .GetMethods()
                    .First(m => m.Name == nameof(Result.Failure)
                             && m.IsGenericMethod)
                    .MakeGenericMethod(innerType);

                return (TResponse)failureMethod.Invoke(null, new object[] { error })!;
            }

            // Non-generic Result
            return (TResponse)(object)Result.Failure(error);
        }

        return await next();
    }

}
