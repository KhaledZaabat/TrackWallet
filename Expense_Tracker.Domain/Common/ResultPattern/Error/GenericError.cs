namespace Expense_Tracker.Domain.Common.ResultPattern.Error
{
    public sealed record GenericError(string Type, string Description)
        : Error(DomainErrorCode.Unexpected, Type, Description)
    {
        public static GenericError Unknown(string description = "Unknown error") =>
            new("Generic.Unknown", description);

        public static GenericError BadRequest(string description = "Bad request") =>
            new("Generic.BadRequest", description);
    }
}