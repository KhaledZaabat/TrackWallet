using Asp.Versioning;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.Identity.Commands.ConfirmAccount;
using Expense_Tracker.Application.Features.Identity.Commands.ForgotPassword;
using Expense_Tracker.Application.Features.Identity.Commands.Logout;
using Expense_Tracker.Application.Features.Identity.Commands.ResendConfirmation;
using Expense_Tracker.Application.Features.Identity.Commands.ResetPassword;
using Expense_Tracker.Application.Features.Identity.Commands.VerifyOtp;
using Expense_Tracker.Application.Features.Login;
using Expense_Tracker.Application.Features.Refresh;
using Expense_Tracker.Application.Features.Register;
using Expense_Tracker.Contracts.Reponses.Identity;
using Expense_Tracker.Contracts.Requests.Identity;
using Expense_Tracker.Domain.Common.ResultPattern.Result;
using MediatR;
using Microsoft.AspNetCore.Mvc;
namespace Expense_Tracker.App.Controllers;









/// <summary>
/// Handles all identity-related actions such as login, token refresh,
/// email confirmation, password reset, and verification flows.
/// </summary>
[ApiController]
[Route("api/identity")]
[Tags("Identity")]
[Produces("application/json")]
[Consumes("application/json")]
[ApiVersion("1.0")]

public sealed class IdentityController(ISender sender) : ControllerBase
{


    /// <summary>
    /// Authenticates a user using email + password credentials.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Authenticates a user by email.")]
    [EndpointDescription("Validates email and password and returns a JWT + Refresh token pair as http only cookie  .")]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        LoginCommand command = new(
            request.Email,
            request.Password,
            request.DeviceId,
            request.FcmToken);

        Result<AuthResponse> result = await sender.Send(command, ct);

        return result.ToActionResult(HttpContext);
    }



    /// <summary>
    /// Registers a new user using email credentials.
    /// </summary>
    /// <remarks>
    /// This endpoint creates a new user account using the provided email and password.
    /// If successful, an OTP (One-Time Password) is sent to the user's email address
    /// for account verification.
    /// </remarks>
    /// <param name="request">
    /// The registration payload containing user credentials and profile information.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Returns 200 OK if registration succeeds.
    /// </returns>
    [HttpPost("register")]
    [Consumes("multipart/form-data")] // Add this
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Register user by email")]
    [EndpointDescription("Creates a new user account using email credentials and sends a verification OTP.")]
    [EndpointName("Register")]
    public async Task<IActionResult> Register(
        [FromForm] RegisterRequest request,
        CancellationToken ct)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.UserName,
            request.FullName,
            request.BirthDate,
            request.IsMale,
            request.ProfileImage
        );

        Result result = await sender.Send(command, ct);
        return result.ToActionResult(HttpContext);
    }



    /// <summary>
    /// Resends account confirmation OTP to email or phone.
    /// </summary>
    /// <param name="request">Object containing email or phone.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success if OTP resent, or failure if already confirmed or user not found.</returns>
    [HttpPost("confirm-account/otp/resend")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Resends the confirmation OTP code to email .")]
    [EndpointDescription("Allows users to request another OTP for confirming their account.")]
    [EndpointName("ResendConfirmationOtp")]
    public async Task<IActionResult> ResendConfirmationOtp(
        [FromBody] ResendConfirmationRequest request,
        CancellationToken ct)
    {
        var command = new ResendConfirmationCommand(request.Email);

        Result result = await sender.Send(command, ct);
        return result.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Confirms a user account (email) using a valid OTP.
    /// </summary>
    /// <param name="request">Request containing email/phone and OTP code.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An IActionResult indicating success or failure.</returns>
    /// <response code="200">Account successfully confirmed.</response>
    /// <response code="400">Invalid data or expired OTP.</response>
    /// <response code="404">User not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("confirm-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Confirms a user account using OTP.")]
    [EndpointDescription("Validates OTP for email or phone and activates the user account.")]
    [EndpointName("ConfirmAccount")]
    public async Task<IActionResult> ConfirmAccount([FromBody] ConfirmAccountRequest request, CancellationToken ct)
    {
        var command = new ConfirmAccountCommand(request.Email, request.Otp);

        Result result = await sender.Send(command, ct);
        return result.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Refreshes the access and refresh tokens.
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Refreshes JWT token pair.")]
    public async Task<ActionResult<AuthResponse>> RefreshToken(
    [FromBody] RefreshTokenRequest request,
    CancellationToken ct)
    {

        RefreshTokenCommand command = new(
            request.RefreshToken,
            request.DeviceId,
            request.FcmToken);

        Result<AuthResponse> result = await sender.Send(command, ct);

        return result.ToActionResult(HttpContext);
    }




    /// <summary>
    /// Sends and Resend an OTP code to the user for resetting their password.
    /// </summary>
    /// <param name="request">Object containing the email of the user.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating whether the OTP was successfully sent.
    /// </returns>
    /// <response code="200">OTP successfully sent to email or phone.</response>
    /// <response code="400">Invalid email/phone format or validation failure.</response>
    /// <response code="404">User not found.</response>
    /// <response code="409">User email or phone is not confirmed.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("reset-password/otp/send")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Sends OTP to email  for resetting the password.")]
    [EndpointDescription("Generates and sends a password-reset OTP to the provided email .")]
    [EndpointName("SendResetPasswordOtp")]
    public async Task<IActionResult> SendResetPasswordOtp(
        [FromBody] ResetPasswordOtpSendRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordOtpSendCommand(request.Email);
        Result res = await sender.Send(command, cancellationToken);
        return res.ToActionResult(HttpContext);
    }


    /// <summary>
    /// Verifies a password-reset OTP sent to the user.
    /// </summary>
    /// <param name="request">Object containing email/phone and the OTP code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating whether the OTP is valid.
    /// </returns>
    /// <response code="200">OTP is valid. User may now reset the password.</response>
    /// <response code="400">Invalid OTP format or expired OTP.</response>
    /// <response code="404">User or OTP key not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("reset-password/otp/verify")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Verifies OTP for resetting the password.")]
    [EndpointDescription("Checks whether a password-reset OTP is correct and has not expired.")]
    [EndpointName("VerifyResetPasswordOtp")]
    public async Task<IActionResult> VerifyResetPasswordOtp(
        [FromBody] VerifyOtpRequest request,
        CancellationToken cancellationToken)
    {
        var command = new VerifyOtpCommand(request.Email, request.Otp);
        Result res = await sender.Send(command, cancellationToken);
        return res.ToActionResult(HttpContext);
    }




    /// <summary>
    /// Resets the user password after OTP verification.
    /// </summary>
    /// <param name="request">Object containing email/phone and the new password.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> indicating whether the password was successfully reset.
    /// </returns>
    /// <response code="200">Password successfully reset.</response>
    /// <response code="400">Weak password or validation failure.</response>
    /// <response code="404">User not found.</response>
    /// <response code="409">User has not confirmed email/phone.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Resets the user's password.")]
    [EndpointDescription("Allows users to set a new password once OTP verification succeeds.")]
    [EndpointName("ResetPassword")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        string userIpAddress = HttpContext.GetClientIp();

        var command = new ResetPasswordCommand(request.Email, request.NewPassword, userIpAddress);
        Result result = await sender.Send(command, cancellationToken);
        return result.ToActionResult(HttpContext);
    }




    /// <summary>
    /// Logs out the current user by revoking their latest active refresh token.
    /// </summary>
    /// <remarks>
    /// This endpoint invalidates the user's most recent refresh token, effectively logging them out.
    /// </remarks>
    /// <response code="200">Logout successful.</response>
    /// <response code="401">User not authenticated.</response>
    /// <response code="500">Unexpected server error.</response>
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Logs out the current user.")]
    [EndpointDescription("Revokes the user's latest refresh token, invalidating their session.")]
    [EndpointName("Logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
    {
        Result result = await sender.Send(new LogoutCommand(request.DeviceId, request.FcmToken), ct);
        return result.ToActionResult(HttpContext);
    }



}