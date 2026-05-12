using Asp.Versioning;
using ErrorOr;
using Expense_Tracker.App.Auth;
using Expense_Tracker.Application.Common.Settings;
using Expense_Tracker.Application.Features;
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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Wolverine;

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
public sealed class IdentityController(
    IMessageBus bus,
    IAuthCookieWriter authCookies,
    IOptionsMonitor<AuthCookieOptions> cookieOptions
) : ControllerBase
{
    /// <summary>
    /// Authenticates a user using email + password credentials.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Authenticates a user by email.")]
    [EndpointDescription(
        "Validates email and password and returns a JWT + Refresh token pair as http only cookie  ."
    )]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct
    )
    {
        LoginCommand command = new(
            request.Email,
            request.Password,
            request.DeviceId,
            request.FcmToken
        );

       ErrorOr<AuthCommandResult> result = await bus.InvokeAsync<ErrorOr<AuthCommandResult>>(
            command,
            ct
        );

        if (result.IsError)
            return this.Problem(result.Errors);

        AuthCommandResult value = result.Value;

        // R2.1, R3.1, R12.2 — issue all three auth cookies through the single writer (R22.10).
        authCookies.WriteAccessCookie(HttpContext, value.AccessToken, value.AccessExpiresAt);
        authCookies.WriteRefreshCookie(HttpContext, value.RefreshToken, value.RefreshExpiresAt);
        authCookies.IssueCsrfCookie(HttpContext);

        return value.Response;
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
    [AllowAnonymous]
    [HttpPost("register")]
    [Consumes("multipart/form-data")] // Add this
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Register user by email")]
    [EndpointDescription(
        "Creates a new user account using email credentials and sends a verification OTP."
    )]
    [EndpointName("Register")]
    public async Task<IActionResult> Register(
        [FromForm] RegisterRequest request,
        CancellationToken ct
    )
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

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(command, ct);
        return result.ToActionResult(this);
    }

    /// <summary>
    /// Resends account confirmation OTP to email or phone.
    /// </summary>
    /// <param name="request">Object containing email or phone.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Success if OTP resent, or failure if already confirmed or user not found.</returns>
    [AllowAnonymous]
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
        CancellationToken ct
    )
    {
        var command = new ResendConfirmationCommand(request.Email);

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(command, ct);
        return result.ToActionResult(this);
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
    [AllowAnonymous]
    [HttpPost("confirm-account")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Confirms a user account using OTP.")]
    [EndpointDescription("Validates OTP for email or phone and activates the user account.")]
    [EndpointName("ConfirmAccount")]
    public async Task<IActionResult> ConfirmAccount(
        [FromBody] ConfirmAccountRequest request,
        CancellationToken ct
    )
    {
        var command = new ConfirmAccountCommand(request.Email, request.Otp);

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(command, ct);
        return result.ToActionResult(this);
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
        CancellationToken ct
    )
    {
        AuthCookieOptions cookieOpts = cookieOptions.CurrentValue;

        // R15.1, R15.3 — raw refresh MUST come from the cookie; the body is ignored.
        string? rawRefresh = HttpContext.Request.Cookies[cookieOpts.RefreshCookieName];
        if (string.IsNullOrEmpty(rawRefresh))
            return Unauthorized();

        RefreshTokenCommand command = new(rawRefresh, request.FcmToken);

        ErrorOr<AuthCommandResult> result = await bus.InvokeAsync<ErrorOr<AuthCommandResult>>(
            command,
            ct
        );

        if (result.IsError)
            return this.Problem(result.Errors);

        AuthCommandResult value = result.Value;

        // R2.2, R3.2, R12.2 — rotate cookies and refresh CSRF on success.
        authCookies.WriteAccessCookie(HttpContext, value.AccessToken, value.AccessExpiresAt);
        authCookies.WriteRefreshCookie(HttpContext, value.RefreshToken, value.RefreshExpiresAt);
        authCookies.RefreshCsrfCookie(HttpContext);

        return value.Response;
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
    [AllowAnonymous]
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
        CancellationToken cancellationToken
    )
    {
        var command = new ResetPasswordOtpSendCommand(request.Email);
        ErrorOr<Success> res = await bus.InvokeAsync<ErrorOr<Success>>(command, cancellationToken);
        return res.ToActionResult(this);
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
    [AllowAnonymous]
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
        CancellationToken cancellationToken
    )
    {
        var command = new VerifyOtpCommand(request.Email, request.Otp);
        ErrorOr<Success> res = await bus.InvokeAsync<ErrorOr<Success>>(command, cancellationToken);
        return res.ToActionResult(this);
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
    [AllowAnonymous]
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
        CancellationToken cancellationToken
    )
    {
        string userIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var command = new ResetPasswordCommand(request.Email, request.NewPassword, userIpAddress);
        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            command,
            cancellationToken
        );
        return result.ToActionResult(this);
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
        // R14.4 — set the skip marker BEFORE dispatching so SilentRefreshMiddleware
        // does not attempt a rotation on the response path for this request.
        HttpContext.Items["AuthLogoutInProgress"] = true;

        ErrorOr<Success> result = await bus.InvokeAsync<ErrorOr<Success>>(
            new LogoutCommand(request.DeviceId, request.FcmToken),
            ct
        );

        if (result.IsError)
            return this.Problem(result.Errors);

        // R14.2 — clear access + refresh + CSRF with the exact attributes used on write (R22.9).
        authCookies.ClearAuthCookies(HttpContext);

        return Ok();
    }
}
