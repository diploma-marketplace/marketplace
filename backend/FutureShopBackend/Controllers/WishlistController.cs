using FutureShopBackend.Application.DTO.Commerce;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages the buyer's wishlist with price-change tracking since addition.
/// </summary>
[ApiController]
[Route("api/wishlist")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class WishlistController(IWishlistService wishlistService) : ControllerBase
{
    /// <summary>
    /// Retrieves the authenticated user's wishlist items including price-drop indicators.
    /// </summary>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<WishlistItemDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<WishlistItemDto>>> GetMyWishlist(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var wishlist = await wishlistService.GetMyWishlistAsync(query, cancellationToken);
        return Ok(wishlist);
    }

    /// <summary>
    /// Adds a product to the authenticated user's wishlist and records its current price for future price-drop tracking.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{productId:guid}")]
    [ProducesResponseType(typeof(WishlistItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<WishlistItemDto>> AddToWishlist(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var item = await wishlistService.AddToWishlistAsync(productId, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, item);
    }

    /// <summary>
    /// Removes a product from the authenticated user's wishlist.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWishlist(
        Guid productId,
        CancellationToken cancellationToken)
    {
        await wishlistService.RemoveFromWishlistAsync(productId, cancellationToken);
        return NoContent();
    }
}
