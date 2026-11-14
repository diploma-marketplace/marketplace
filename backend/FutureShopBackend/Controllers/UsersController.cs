using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Users;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages the authenticated user's private profile, password, seller verification requests, and seller analytics.
/// </summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class UsersController(IUserService userService) : ControllerBase
{
    /// <summary>
    /// Retrieves the full private profile of the currently authenticated user or company.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserProfileResponse>> GetMyProfile(CancellationToken cancellationToken)
    {
        var profile = await userService.GetMyProfileAsync(cancellationToken);
        return Ok(profile);
    }

    /// <summary>
    /// Updates personal details, contact info, social links, and UI preferences (language/theme) of the authenticated user.
    /// </summary>
    /// <param name="request">Updated profile fields.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
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
    /// Changes the password of the currently authenticated account.
    /// </summary>
    /// <param name="request">Current and new password.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
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
    /// Submits FOP (sole proprietor) or Company credentials for moderator verification to obtain the "Verified Seller" badge and higher limits.
    /// </summary>
    /// <param name="request">Tax code (RNOKPP/EDRPOU), legal address, and IBAN details.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("me/verification")]
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
    /// Returns sales, view, wishlist, and conversion analytics for the authenticated seller or company.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("me/analytics")]
    [ProducesResponseType(typeof(SellerAnalyticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<SellerAnalyticsResponse>> GetMySellerAnalytics(CancellationToken cancellationToken)
    {
        var analytics = await userService.GetMySellerAnalyticsAsync(cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Deactivates the currently authenticated user account upon owner request.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("me")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeactivateMyAccount(CancellationToken cancellationToken)
    {
        await userService.DeactivateMyAccountAsync(cancellationToken);
        return NoContent();
    }
}
