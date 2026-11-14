using FutureShopBackend.Application.DTO.Auth;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages user and company registration, session cookie authentication, contact verification, and password recovery.
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class AuthController(IAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user (buyer / private seller / FOP) or company account and establishes a session cookie.
    /// </summary>
    /// <param name="request">Registration credentials, role selection, and optional company details.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthSessionResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var session = await authService.RegisterAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCurrentSession), session);
    }

    /// <summary>
    /// Authenticates a user or company by email and password and issues an HTTP-only Session Cookie.
    /// </summary>
    /// <param name="request">Login credentials.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthSessionResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var session = await authService.LoginAsync(request, cancellationToken);
        return Ok(session);
    }

    /// <summary>
    /// Terminates the active session and clears the authentication cookie.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MessageResponse>> Logout(CancellationToken cancellationToken)
    {
        var result = await authService.LogoutAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Returns the currently authenticated user's session, role, and UI preferences.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthSessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthSessionResponse>> GetCurrentSession(CancellationToken cancellationToken)
    {
        var session = await authService.GetCurrentSessionAsync(cancellationToken);
        return Ok(session);
    }

    /// <summary>
    /// Confirms an email address or phone number using a one-time verification code.
    /// </summary>
    /// <param name="request">Verification channel and one-time code.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("verify-contact")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MessageResponse>> VerifyContact(
        [FromBody] VerifyContactRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.VerifyContactAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Initiates the password recovery flow by sending a reset token to the registered email.
    /// </summary>
    /// <param name="request">Account email address.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.ForgotPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Resets the user's password using a valid recovery token.
    /// </summary>
    /// <param name="request">Email, reset token, and new password.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<MessageResponse>> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.ResetPasswordAsync(request, cancellationToken);
        return Ok(result);
    }
}
