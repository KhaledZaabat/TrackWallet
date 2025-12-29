using Asp.Versioning;
using Expense_Tracker.App.Helpers;
using Expense_Tracker.Application.Features.ForgotPassword;
using Expense_Tracker.Application.Features.Login;
using Expense_Tracker.Application.Features.Refresh;
using Expense_Tracker.Application.Features.Register;
using Expense_Tracker.Application.Features.ResetPassword;
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
            request.DeviceId);

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
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Register user by email")]
    [EndpointDescription("Creates a new user account using email credentials and sends a verification OTP.")]
    [EndpointName("Register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
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




    // -------------------------------------------------------------------------
    // REFRESH TOKEN
    // -------------------------------------------------------------------------

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
            request.DeviceId);

        Result<AuthResponse> result = await sender.Send(command, ct);

        return result.ToActionResult(HttpContext);
    }
    /// <summary>
    /// Sends a password reset code to the user's email.
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Generates password reset code and sends it to email.")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgetPasswordRequest request,
        CancellationToken ct)
    {
        var command = new ForgetPasswordCommand(request.Email);
        Result result = await sender.Send(command, ct);

        return result.ToActionResult(HttpContext);
    }

    // -------------------------------------------------------------------------
    // RESET PASSWORD WITH CODE (NEW)
    // -------------------------------------------------------------------------

    /// <summary>
    /// Resets the user password using email + new password.
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    [EndpointSummary("Resets the user's password.")]
    [EndpointDescription("Allows the user to set a new password using a valid OTP code.")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken ct)
    {
        var userIpAddress = HttpContext.GetClientIp();

        var command = new ResetPasswordWithCodeCommand(
            request.UserId,
            request.Code,
            request.NewPassword,
            userIpAddress
        );

        Result result = await sender.Send(command, ct);
        return result.ToActionResult(HttpContext);
    }
}