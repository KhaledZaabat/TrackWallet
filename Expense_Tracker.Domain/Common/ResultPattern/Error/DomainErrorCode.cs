namespace Expense_Tracker.Domain.Common.ResultPattern.Error;

public sealed record DomainErrorCode : ErrorCode
{
    private DomainErrorCode(string value) : base(value) { }

    public static readonly DomainErrorCode NotFound =
        new("domain.not_found");

    public static readonly DomainErrorCode Conflict =
        new("domain.conflict");

    public static readonly DomainErrorCode InvalidState =
        new("domain.invalid_state");

    public static readonly DomainErrorCode Validation =
        new("domain.validation");

    public static readonly DomainErrorCode Unexpected =
        new("domain.unexpected");

    public static readonly DomainErrorCode None =
        new("domain.none");
    public static readonly DomainErrorCode Forbidden =
     new("domain.forbidden");
}
