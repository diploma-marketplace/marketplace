using System.ComponentModel.DataAnnotations;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.DTO.Catalog;

/// <summary>
/// Query parameters for searching, filtering, and sorting products in the catalog.
/// </summary>
public sealed record CatalogSearchQuery : PaginationQuery
{
    /// <summary>Search keyword matching product title, category, or tags.</summary>
    [StringLength(200)]
    public string? Query { get; init; }

    /// <summary>Filter by category identifier.</summary>
    public Guid? CategoryId { get; init; }

    /// <summary>Filter by brand identifier.</summary>
    public Guid? BrandId { get; init; }

    /// <summary>Filter by specific seller identifier.</summary>
    public Guid? SellerId { get; init; }

    /// <summary>
    /// Filter separating first-party Marketplace offers from third-party seller/user listings.
    /// </summary>
    public SellerType? SellerType { get; init; }

    /// <summary>Filter by physical condition (New or Used).</summary>
    public ProductCondition? Condition { get; init; }

    /// <summary>Minimum price in UAH.</summary>
    [Range(0, double.MaxValue)]
    public decimal? MinPrice { get; init; }

    /// <summary>Maximum price in UAH.</summary>
    [Range(0, double.MaxValue)]
    public decimal? MaxPrice { get; init; }

    /// <summary>Return only products currently in stock.</summary>
    public bool? InStockOnly { get; init; }

    /// <summary>
    /// Dynamic technical specification filters keyed by attribute name (e.g. Attributes[RAM]=16GB).
    /// </summary>
    public Dictionary<string, string>? Attributes { get; init; }

    /// <summary>Sorting criterion (Relevance, PriceAsc, PriceDesc, RatingDesc, Newest, Popular).</summary>
    public CatalogSortBy SortBy { get; init; } = CatalogSortBy.Relevance;
}

/// <summary>
/// Real-time search autocomplete response containing matching queries, categories, and product previews.
/// </summary>
public sealed record SearchSuggestionsResponse
{
    /// <summary>Suggested search phrases or tags.</summary>
    public IReadOnlyList<string> SuggestedQueries { get; init; } = [];

    /// <summary>Matching categories.</summary>
    public IReadOnlyList<CategorySuggestionItem> Categories { get; init; } = [];

    /// <summary>Top matching product previews.</summary>
    public IReadOnlyList<ProductCardDto> Products { get; init; } = [];
}

/// <summary>
/// Category shortcut inside search autocomplete suggestions.
/// </summary>
public sealed record CategorySuggestionItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
}

/// <summary>
/// Compact seller summary embedded in product cards and detail pages.
/// </summary>
public sealed record ProductSellerSummaryDto
{
    public Guid Id { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public SellerType SellerType { get; init; }
    public bool IsVerifiedSeller { get; init; }
    public double Rating { get; init; }
    public int ReviewsCount { get; init; }
}

/// <summary>
/// Dynamic specification key-value pair on a product.
/// </summary>
public sealed record ProductAttributeValueDto
{
    public Guid AttributeId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public string? Unit { get; init; }
}

/// <summary>
/// Compact product card representation used in catalog grids, search results, and recommendations.
/// </summary>
public sealed record ProductCardDto
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string? PrimaryImageUrl { get; init; }
    public decimal PriceUah { get; init; }
    public decimal? PreviousPriceUah { get; init; }
    public ProductCondition Condition { get; init; }
    public bool IsInStock { get; init; }
    public int StockQuantity { get; init; }
    public double Rating { get; init; }
    public int ReviewsCount { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public ProductSellerSummaryDto Seller { get; init; } = new();
}

/// <summary>
/// Full Product Detail Page (PDP) response with gallery, specifications, seller info, and stock status.
/// </summary>
public sealed record ProductDetailsResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Slug { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public decimal PriceUah { get; init; }
    public decimal? PreviousPriceUah { get; init; }
    public ProductCondition Condition { get; init; }
    public ProductStatus Status { get; init; }
    public int StockQuantity { get; init; }
    public bool IsInStock => StockQuantity > 0;
    public double Rating { get; init; }
    public int ReviewsCount { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public Guid? BrandId { get; init; }
    public string? BrandName { get; init; }
    public IReadOnlyList<string> ImageUrls { get; init; } = [];
    public IReadOnlyList<string> Tags { get; init; } = [];
    public IReadOnlyList<ProductAttributeValueDto> Specifications { get; init; } = [];
    public ProductSellerSummaryDto Seller { get; init; } = new();
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Lightweight response for the Quick View modal window without leaving the catalog page.
/// </summary>
public sealed record ProductQuickViewResponse
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public decimal PriceUah { get; init; }
    public decimal? PreviousPriceUah { get; init; }
    public ProductCondition Condition { get; init; }
    public bool IsInStock { get; init; }
    public int StockQuantity { get; init; }
    public double Rating { get; init; }
    public int ReviewsCount { get; init; }
    public IReadOnlyList<string> ImageUrls { get; init; } = [];
    public string ShortDescription { get; init; } = string.Empty;
    public IReadOnlyList<ProductAttributeValueDto> KeySpecifications { get; init; } = [];
    public ProductSellerSummaryDto Seller { get; init; } = new();
}

/// <summary>
/// Single chronological price point for the Dynamic Price History chart.
/// </summary>
public sealed record PriceHistoryPointDto
{
    public DateOnly Date { get; init; }
    public decimal PriceUah { get; init; }
}

/// <summary>
/// Dynamic price history analytics for a product over 30, 90, or 365 days.
/// </summary>
public sealed record ProductPriceHistoryResponse
{
    public Guid ProductId { get; init; }
    public int PeriodDays { get; init; }
    public decimal CurrentPriceUah { get; init; }
    public decimal LowestPriceUah { get; init; }
    public decimal HighestPriceUah { get; init; }
    public decimal AveragePriceUah { get; init; }
    public IReadOnlyList<PriceHistoryPointDto> Points { get; init; } = [];
}

/// <summary>
/// Row in the product comparison matrix representing a single specification across compared products.
/// </summary>
public sealed record ComparisonSpecificationRowDto
{
    public Guid AttributeId { get; init; }
    public string AttributeName { get; init; } = string.Empty;
    public string? Unit { get; init; }

    /// <summary>Indicates whether the compared products have different values for this specification.</summary>
    public bool HasDifference { get; init; }

    /// <summary>Map of ProductId to formatted attribute value.</summary>
    public IReadOnlyDictionary<Guid, string?> ValuesByProductId { get; init; } = new Dictionary<Guid, string?>();
}

/// <summary>
/// Tabular product comparison response highlighting specification differences.
/// </summary>
public sealed record ProductComparisonResponse
{
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public IReadOnlyList<ProductCardDto> Products { get; init; } = [];
    public IReadOnlyList<ComparisonSpecificationRowDto> SpecificationRows { get; init; } = [];
}

/// <summary>
/// Specification input item when creating or updating a product listing.
/// </summary>
public sealed record UpsertProductSpecificationRequest
{
    [Required]
    public Guid AttributeId { get; init; }

    [Required]
    [StringLength(500)]
    public required string Value { get; init; }
}

/// <summary>
/// Request payload for creating or updating a product listing (User, Company, or Admin/Marketplace).
/// </summary>
public sealed record UpsertProductRequest
{
    [Required]
    [StringLength(200, MinimumLength = 3)]
    public required string Title { get; init; }

    [Required]
    [StringLength(10000, MinimumLength = 10)]
    public required string Description { get; init; }

    [Required]
    public Guid CategoryId { get; init; }

    public Guid? BrandId { get; init; }

    [Required]
    [Range(0.01, 100000000)]
    public decimal PriceUah { get; init; }

    [Required]
    public ProductCondition Condition { get; init; } = ProductCondition.New;

    [Required]
    [Range(0, 1000000)]
    public int StockQuantity { get; init; } = 1;

    [Required]
    [MinLength(1)]
    [MaxLength(10)]
    public required IReadOnlyList<string> ImageUrls { get; init; }

    [MaxLength(20)]
    public IReadOnlyList<string> Tags { get; init; } = [];

    public IReadOnlyList<UpsertProductSpecificationRequest> Specifications { get; init; } = [];
}

/// <summary>
/// Request payload for updating the visibility/publication status of a product listing.
/// </summary>
public sealed record UpdateProductStatusRequest
{
    [Required]
    public ProductStatus Status { get; init; }
}

/// <summary>
/// Engagement statistics for a seller's product listing.
/// </summary>
public sealed record ProductStatisticsResponse
{
    public Guid ProductId { get; init; }
    public long ViewsCount { get; init; }
    public int WishlistAdditionsCount { get; init; }
    public int CompletedSalesCount { get; init; }
}

/// <summary>
/// Request metadata for bulk catalog import from CSV/XLSX/XML feed (Company and Admin roles).
/// </summary>
public sealed record BulkProductImportRequest
{
    [Required]
    public ImportFeedFormat Format { get; init; }

    /// <summary>URL or uploaded file reference of the catalog feed.</summary>
    [Required]
    [StringLength(1000)]
    public required string FeedSourceUrl { get; init; }

    /// <summary>Whether existing SKUs should be updated with new prices and stock quantities.</summary>
    public bool OverwriteExisting { get; init; } = true;
}

/// <summary>
/// Summary report returned after processing a bulk catalog import.
/// </summary>
public sealed record BulkProductImportResultResponse
{
    public Guid ImportJobId { get; init; }
    public int TotalRowsProcessed { get; init; }
    public int CreatedCount { get; init; }
    public int UpdatedCount { get; init; }
    public int FailedCount { get; init; }
    public IReadOnlyList<string> RowErrors { get; init; } = [];
}
