namespace FutureShopBackend.Domain.Enums;

/// <summary>
/// Physical condition of a listed product.
/// </summary>
public enum ProductCondition
{
    New,
    Used
}

/// <summary>
/// Publication and moderation status of a product listing.
/// </summary>
public enum ProductStatus
{
    Draft,
    PendingModeration,
    Active,
    Hidden,
    Rejected,
    Archived
}

/// <summary>
/// Supported sorting options for catalog search results.
/// </summary>
public enum CatalogSortBy
{
    Relevance,
    PriceAsc,
    PriceDesc,
    RatingDesc,
    Newest,
    Popular
}

/// <summary>
/// Data type of a dynamic category attribute used for filtering and comparison.
/// </summary>
public enum AttributeDataType
{
    Text,
    Number,
    Boolean,
    Select
}

/// <summary>
/// Supported file formats for bulk catalog import.
/// </summary>
public enum ImportFeedFormat
{
    Csv,
    Xlsx,
    Xml
}
