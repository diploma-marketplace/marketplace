using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Users;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Public endpoints for viewing seller and company profiles and ratings.
/// </summary>
[ApiController]
[Route("api/sellers")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class SellersController(ISellerService sellerService) : ControllerBase
{
    /// <summary>
    /// Retrieves the public profile of a seller (individual, FOP, Company, or Marketplace).
    /// </summary>
    /// <param name="sellerId">Unique seller identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{sellerId:guid}")]
    [ProducesResponseType(typeof(PublicSellerProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PublicSellerProfileResponse>> GetSellerProfile(
        Guid sellerId,
        CancellationToken cancellationToken)
    {
        var profile = await sellerService.GetSellerProfileAsync(sellerId, cancellationToken);
        return Ok(profile);
    }
}
