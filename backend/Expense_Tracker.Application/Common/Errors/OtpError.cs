using Expense_Tracker.Domain.Common.ResultPattern.Error;

namespace Expense_Tracker.Application.Common.Errors;

public sealed record OtpError(ApplicationErrorCode ApplicationErrorCode, string Type, string Description)
    : Error(ApplicationErrorCode, Type, Description)
{

    /// <summary>
    /// OTP is invalid or has expired.
    /// </summary>
    public static OtpError InvalidOrExpired(string description = "Invalid or expired OTP code") =>
     new(ApplicationErrorCode.Validation,
         "Otp.InvalidOrExpired",
         description);

    /// <summary>
    /// User attempted to generate an OTP but one already exists and has not expired.
    /// </summary>
    public static OtpError NotExpired(string description = "An active OTP already exists. Please wait before requesting a new one.") =>
        new(ApplicationErrorCode.Conflict,
            "Otp.NotExpired",
            description);


}
