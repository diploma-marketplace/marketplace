using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages product catalog search, filtering (Marketplace vs ThirdParty sellers), Quick View, dynamic price history, comparison, and seller CRUD.
/// </summary>
[ApiController]
[Route("api/products")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class ProductsController(IProductService productService) : ControllerBase
{
    /// <summary>
    /// Searches, filters, and sorts products across the marketplace catalog.
    /// Supports filtering by category, price range, condition, dynamic specifications, and SellerType (Marketplace vs ThirdParty).
    /// </summary>
    /// <param name="query">Search, filter, sort, and pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ProductCardDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<ProductCardDto>>> SearchProducts(
        [FromQuery] CatalogSearchQuery query,
        CancellationToken cancellationToken)
    {
        var results = await productService.SearchProductsAsync(query, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Returns real-time autocomplete suggestions (queries, categories, and top products) as the user types in the search bar.
    /// </summary>
    /// <param name="q">Search prefix typed by the user.</param>
    /// <param name="limit">Maximum number of suggestions to return.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(SearchSuggestionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchSuggestionsResponse>> GetSearchSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var suggestions = await productService.GetSearchSuggestionsAsync(q, limit, cancellationToken);
        return Ok(suggestions);
    }

    /// <summary>
    /// Returns personalized product recommendations based on user preferences and browsing behavior cookies.
    /// </summary>
    /// <param name="limit">Maximum number of recommended products to return.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(IReadOnlyList<ProductCardDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductCardDto>>> GetRecommendations(
        [FromQuery] int limit = 12,
        CancellationToken cancellationToken = default)
    {
        var recommendations = await productService.GetRecommendationsAsync(limit, cancellationToken);
        return Ok(recommendations);
    }

    /// <summary>
    /// Compares multiple products side-by-side by their dynamic category specifications and highlights differences.
    /// </summary>
    /// <param name="ids">List of product identifiers to compare (minimum 2).</param>
    /// <param name="onlyDifferences">If true, returns only specification rows where product values differ.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("compare")]
    [ProducesResponseType(typeof(ProductComparisonResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductComparisonResponse>> CompareProducts(
        [FromQuery] IReadOnlyList<Guid> ids,
        [FromQuery] bool onlyDifferences = false,
        CancellationToken cancellationToken = default)
    {
        var comparison = await productService.CompareProductsAsync(ids, onlyDifferences, cancellationToken);
        return Ok(comparison);
    }

    /// <summary>
    /// Retrieves full Product Detail Page (PDP) information including images, dynamic specifications, and seller summary.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{productId:guid}")]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsResponse>> GetProductById(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await productService.GetProductByIdAsync(productId, cancellationToken);
        return Ok(product);
    }

    /// <summary>
    /// Retrieves lightweight product summary and key specifications for the Quick View modal window.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{productId:guid}/quick-view")]
    [ProducesResponseType(typeof(ProductQuickViewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductQuickViewResponse>> GetProductQuickView(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var quickView = await productService.GetProductQuickViewAsync(productId, cancellationToken);
        return Ok(quickView);
    }

    /// <summary>
    /// Retrieves historical price changes for the Dynamic Price History chart (supports 30, 90, or 365 days).
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="periodDays">History window in days (30, 90, or 365).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{productId:guid}/price-history")]
    [ProducesResponseType(typeof(ProductPriceHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceHistoryResponse>> GetProductPriceHistory(
        Guid productId,
        [FromQuery] int periodDays = 90,
        CancellationToken cancellationToken = default)
    {
        var history = await productService.GetProductPriceHistoryAsync(productId, periodDays, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Publishes a new product listing (available to User/FOP within active limit, verified Company, and Admin/Marketplace).
    /// </summary>
    /// <param name="request">Product details, price, condition, stock, images, and specifications.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDetailsResponse>> CreateProduct(
        [FromBody] UpsertProductRequest request,
        CancellationToken cancellationToken)
    {
        var created = await productService.CreateProductAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetProductById), new { productId = created.Id }, created);
    }

    /// <summary>
    /// Updates an existing product listing owned by the authenticated seller (or managed by Admin).
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="request">Updated product details.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPut("{productId:guid}")]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsResponse>> UpdateProduct(
        Guid productId,
        [FromBody] UpsertProductRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await productService.UpdateProductAsync(productId, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Changes the publication status of a product listing (e.g. hide, activate, or archive).
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="request">Target publication status.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPatch("{productId:guid}/status")]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsResponse>> UpdateProductStatus(
        Guid productId,
        [FromBody] UpdateProductStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await productService.UpdateProductStatusAsync(productId, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a product listing owned by the authenticated seller (or by Admin).
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(Guid productId, CancellationToken cancellationToken)
    {
        await productService.DeleteProductAsync(productId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retrieves engagement statistics (views, wishlist additions, sales) for a seller's product listing.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{productId:guid}/stats")]
    [ProducesResponseType(typeof(ProductStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductStatisticsResponse>> GetProductStatistics(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var stats = await productService.GetProductStatisticsAsync(productId, cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Performs bulk catalog import or update from a CSV, XLSX, or XML feed (Company and Admin roles only).
    /// </summary>
    /// <param name="request">Feed format and source URL/reference.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("import")]
    [ProducesResponseType(typeof(BulkProductImportResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<BulkProductImportResultResponse>> ImportProducts(
        [FromBody] BulkProductImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await productService.ImportProductsAsync(request, cancellationToken);
        return Ok(result);
    }
}
