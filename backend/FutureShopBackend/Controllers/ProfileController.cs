using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Users;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Endpoints for working with private and public account profile data and seller verification.
/// </summary>
[ApiController]
[Route("api/profile")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProfileController(
    IUserService userService,
    ISellerService sellerService) : ControllerBase
{
    /// <summary>
    /// Retrieves the full private profile of the currently authenticated user or company.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [Authorize(Roles = "User, Company, Admin")]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile(CancellationToken cancellationToken)
    {
        var profile = await userService.GetMyProfileAsync(cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Updates the authenticated user's full name, phone number, avatar, password, and account preferences.
    /// </summary>
    /// <param name="request">Updated profile and optional password change fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [Authorize(Roles = "User, Company, Admin")]
    [HttpPut("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> UpdateMyProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var profile = await userService.UpdateMyProfileAsync(request, cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Changes the password of the currently authenticated user account.
    /// </summary>
    /// <param name="request">Current and new password.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [Authorize(Roles = "User, Company, Admin")]
    [HttpPut("me/password")]
    [ProducesResponseType(typeof(MessageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MessageResponse>> ChangePassword(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userService.ChangePasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submits FOP (RNOKPP) or Company (EDRPOU) verification credentials for moderator approval.
    /// </summary>
    /// <param name="request">Tax identification code (EDRPOU / RNOKPP) and legal details.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [Authorize(Roles = "User, Company")]
    [HttpPost("verify")]
    [ProducesResponseType(typeof(SellerVerificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SellerVerificationResponse>> SubmitVerification(
        [FromBody] SubmitSellerVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await userService.SubmitVerificationAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Retrieves the public profile of a seller (displays only permitted public data without private fields).
    /// </summary>
    /// <param name="id">Unique seller identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PublicSellerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicSellerProfileResponse>> GetSellerPublicProfile(
        int id,
        CancellationToken cancellationToken)
    {
        var profile = await sellerService.GetSellerProfileAsync(id, cancellationToken);
        return Ok(profile);
    }
}
