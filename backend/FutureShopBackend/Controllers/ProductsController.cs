using FutureShopBackend.Api.Attributes;
using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Enums;
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
    [HttpGet("search")]
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
    /// Returns real-time autocomplete suggestions (queries, categories, and top products) as the user types in the search bar (live search).
    /// </summary>
    /// <param name="q">Search prefix typed by the user.</param>
    /// <param name="query">Alternative search prefix query parameter.</param>
    /// <param name="limit">Maximum number of suggestions to return.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("suggestions")]
    [HttpGet("live-search")]
    [ProducesResponseType(typeof(SearchSuggestionsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SearchSuggestionsResponse>> GetSearchSuggestions(
        [FromQuery] string? q = null,
        [FromQuery] string? query = null,
        [FromQuery] int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var prefix = !string.IsNullOrWhiteSpace(q) ? q : (query ?? string.Empty);
        var suggestions = await productService.GetSearchSuggestionsAsync(prefix, limit, cancellationToken);
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
        [FromQuery] IReadOnlyList<int> ids,
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
    [HttpGet("{productId:int}")]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsResponse>> GetProductById(
        int productId,
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
    [HttpGet("{productId:int}/quick-view")]
    [ProducesResponseType(typeof(ProductQuickViewResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductQuickViewResponse>> GetProductQuickView(
        int productId,
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
    [HttpGet("{productId:int}/price-history")]
    [ProducesResponseType(typeof(ProductPriceHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductPriceHistoryResponse>> GetProductPriceHistory(
        int productId,
        [FromQuery] int periodDays = 90,
        CancellationToken cancellationToken = default)
    {
        var history = await productService.GetProductPriceHistoryAsync(productId, periodDays, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Uploads product photos before or during listing creation.
    /// Validates file format (.jpg, .jpeg, .png, .webp), size (max 5 MB), and image header signature.
    /// </summary>
    /// <param name="files">Collection of image files to upload (1 to 10 images).</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [AuthorizeRoles(UserRole.User, UserRole.Company, UserRole.Admin)]
    [HttpPost("images")]
    [HttpPost("upload-image")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IReadOnlyList<string>>> UploadImages(
        [FromForm] IFormFileCollection files,
        CancellationToken cancellationToken)
    {
        var urls = await productService.UploadProductImagesAsync(null, files, cancellationToken);
        return Ok(urls);
    }

    /// <summary>
    /// Uploads and attaches photos directly to an existing product owned by the authenticated seller (or managed by Admin).
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="files">Collection of image files to upload.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [AuthorizeRoles(UserRole.User, UserRole.Company, UserRole.Admin)]
    [HttpPost("{productId:int}/images")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<string>>> UploadProductPhotos(
        int productId,
        [FromForm] IFormFileCollection files,
        CancellationToken cancellationToken)
    {
        var urls = await productService.UploadProductImagesAsync(productId, files, cancellationToken);
        return Ok(urls);
    }

    /// <summary>
    /// Publishes a new product listing (available to User/FOP within active limit, verified Company, and Admin/Marketplace).
    /// </summary>
    /// <param name="request">Product details, price, condition, stock, images, and specifications.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [AuthorizeRoles(UserRole.User, UserRole.Company, UserRole.Admin)]
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
    [AuthorizeRoles(UserRole.User, UserRole.Company, UserRole.Admin)]
    [HttpPut("{productId:int}")]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsResponse>> UpdateProduct(
        int productId,
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
    [AuthorizeRoles(UserRole.User, UserRole.Company, UserRole.Admin)]
    [HttpPatch("{productId:int}/status")]
    [ProducesResponseType(typeof(ProductDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDetailsResponse>> UpdateProductStatus(
        int productId,
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
    [AuthorizeRoles(UserRole.User, UserRole.Company, UserRole.Admin)]
    [HttpDelete("{productId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(int productId, CancellationToken cancellationToken)
    {
        await productService.DeleteProductAsync(productId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retrieves engagement statistics (views, wishlist additions, sales) for a seller's product listing.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [AuthorizeRoles(UserRole.User, UserRole.Company, UserRole.Admin)]
    [HttpGet("{productId:int}/stats")]
    [ProducesResponseType(typeof(ProductStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductStatisticsResponse>> GetProductStatistics(
        int productId,
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
    [AuthorizeRoles(UserRole.Company, UserRole.Admin)]
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
