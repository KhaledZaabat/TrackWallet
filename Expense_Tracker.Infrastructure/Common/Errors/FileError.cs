using Expense_Tracker.Domain.Common.ResultPattern.Error;
using Expense_Tracker.Infrastructure.Common.Errors;

public sealed record FileError(InfrastructureErrorCode InfrastructureErrorCode, string Type, string Description)
    : Error(InfrastructureErrorCode, Type, Description)
{
    public static FileError FileNotFound(string code = "File.NotFound", string detail = $"File with ID was not found.") =>
        new(InfrastructureErrorCode.NotFound, code, detail);
    public static FileError EmptyFile(string code = "File.Empty", string detail = $"File with  is Empty.") =>
      new(InfrastructureErrorCode.NotFound, code, detail);

    public static FileError InvalidFileType(string detail) =>
        new(InfrastructureErrorCode.Validation, "File.InvalidType", detail);

    public static FileError UploadFailed(string detail) =>
        new(InfrastructureErrorCode.ServiceUnavailable, "File.UploadFailed", detail);

    public static FileError DownloadFailed(string detail) =>
        new(InfrastructureErrorCode.ServiceUnavailable, "File.DownloadFailed", detail);

    public static FileError StreamFailed(string detail) =>
        new(InfrastructureErrorCode.ServiceUnavailable, "File.StreamFailed", detail);

    public static FileError FileTooLarge(long maxSize) =>
        new(InfrastructureErrorCode.Validation, "File.TooLarge", $"File exceeds the maximum allowed size of {maxSize} bytes.");

    public static FileError ValidationFailed(string detail) =>
        new(InfrastructureErrorCode.Validation, "File.ValidationFailed", detail);

    public static FileError Unknown(string detail) =>
        new(InfrastructureErrorCode.None, "File.Unknown", detail);
}