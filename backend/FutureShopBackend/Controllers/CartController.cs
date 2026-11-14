using FutureShopBackend.Application.DTO.Commerce;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages the buyer's shopping cart with automatic price calculation and grouping by sellers (User role only).
/// </summary>
[ApiController]
[Route("api/cart")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class CartController(ICartService cartService) : ControllerBase
{
    /// <summary>
    /// Retrieves the authenticated buyer's shopping cart grouped by sellers.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CartResponse>> GetMyCart(CancellationToken cancellationToken)
    {
        var cart = await cartService.GetMyCartAsync(cancellationToken);
        return Ok(cart);
    }

    /// <summary>
    /// Adds a product to the shopping cart (buyers cannot add their own listings; companies cannot act as buyers).
    /// </summary>
    /// <param name="request">Product ID and quantity to add.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("items")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.AddItemAsync(request, cancellationToken);
        return Ok(cart);
    }

    /// <summary>
    /// Updates the quantity of a specific product in the shopping cart.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="request">Updated quantity.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> UpdateItemQuantity(
        Guid productId,
        [FromBody] UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.UpdateItemQuantityAsync(productId, request, cancellationToken);
        return Ok(cart);
    }

    /// <summary>
    /// Removes a specific product from the shopping cart.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(typeof(CartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CartResponse>> RemoveItem(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var cart = await cartService.RemoveItemAsync(productId, cancellationToken);
        return Ok(cart);
    }

    /// <summary>
    /// Clears all items from the authenticated buyer's shopping cart.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
    {
        await cartService.ClearCartAsync(cancellationToken);
        return NoContent();
    }
}
