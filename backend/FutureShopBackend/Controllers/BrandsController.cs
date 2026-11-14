using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages product brands in the catalog.
/// </summary>
[ApiController]
[Route("api/brands")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class BrandsController(IBrandService brandService) : ControllerBase
{
    /// <summary>
    /// Retrieves all brands available in the catalog.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<BrandDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<BrandDto>>> GetBrands(CancellationToken cancellationToken)
    {
        var brands = await brandService.GetBrandsAsync(cancellationToken);
        return Ok(brands);
    }

    /// <summary>
    /// Creates a new brand (Admin role required).
    /// </summary>
    /// <param name="request">Brand creation payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BrandDto>> CreateBrand(
        [FromBody] UpsertBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await brandService.CreateBrandAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetBrands), new { id = brand.Id }, brand);
    }

    /// <summary>
    /// Updates an existing brand (Admin role required).
    /// </summary>
    /// <param name="brandId">Brand identifier.</param>
    /// <param name="request">Updated brand payload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPut("{brandId:guid}")]
    [ProducesResponseType(typeof(BrandDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BrandDto>> UpdateBrand(
        Guid brandId,
        [FromBody] UpsertBrandRequest request,
        CancellationToken cancellationToken)
    {
        var brand = await brandService.UpdateBrandAsync(brandId, request, cancellationToken);
        return Ok(brand);
    }

    /// <summary>
    /// Deletes a brand (Admin role required).
    /// </summary>
    /// <param name="brandId">Brand identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("{brandId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBrand(Guid brandId, CancellationToken cancellationToken)
    {
        await brandService.DeleteBrandAsync(brandId, cancellationToken);
        return NoContent();
    }
}
