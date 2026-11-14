using System.ComponentModel.DataAnnotations;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.DTO.Catalog;

/// <summary>
/// Hierarchical category node with child subcategories.
/// </summary>
public sealed record CategoryTreeDto
{
    /// <summary>Unique category identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Parent category identifier (null for root categories).</summary>
    public Guid? ParentId { get; init; }

    /// <summary>Localized category name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>URL-friendly category slug.</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>Category icon or image URL.</summary>
    public string? IconUrl { get; init; }

    /// <summary>Whether listings in this category are restricted by platform policy.</summary>
    public bool IsRestricted { get; init; }

    /// <summary>Nested child subcategories.</summary>
    public IReadOnlyList<CategoryTreeDto> Children { get; init; } = [];
}

/// <summary>
/// Dynamic specification attribute definition configured for a category.
/// </summary>
public sealed record CategoryAttributeDefinitionDto
{
    /// <summary>Attribute definition identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Category identifier this attribute belongs to.</summary>
    public Guid CategoryId { get; init; }

    /// <summary>Attribute display name (e.g. "Screen Size", "RAM").</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Measurement unit (e.g. "GB", "inch", "Hz").</summary>
    public string? Unit { get; init; }

    /// <summary>Value data type (Text, Number, Boolean, Select).</summary>
    public AttributeDataType DataType { get; init; }

    /// <summary>Whether the attribute can be used as a catalog filter.</summary>
    public bool IsFilterable { get; init; }

    /// <summary>Whether the attribute is included in the product comparison table.</summary>
    public bool IsComparable { get; init; }

    /// <summary>Whether the attribute is required when creating a product in this category.</summary>
    public bool IsRequired { get; init; }

    /// <summary>Allowed option values when DataType is Select.</summary>
    public IReadOnlyList<string> AllowedValues { get; init; } = [];
}

/// <summary>
/// Request payload for creating or updating a catalog category (Admin only).
/// </summary>
public sealed record UpsertCategoryRequest
{
    /// <summary>Optional parent category identifier.</summary>
    public Guid? ParentId { get; init; }

    /// <summary>Category display name.</summary>
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public required string Name { get; init; }

    /// <summary>URL-friendly slug.</summary>
    [Required]
    [StringLength(120, MinimumLength = 2)]
    public required string Slug { get; init; }

    /// <summary>Optional category icon URL.</summary>
    [Url]
    [StringLength(500)]
    public string? IconUrl { get; init; }

    /// <summary>Dynamic specification attributes associated with this category.</summary>
    public IReadOnlyList<UpsertCategoryAttributeRequest> Attributes { get; init; } = [];
}

/// <summary>
/// Definition of a dynamic category attribute inside an upsert request.
/// </summary>
public sealed record UpsertCategoryAttributeRequest
{
    /// <summary>Attribute display name.</summary>
    [Required]
    [StringLength(100)]
    public required string Name { get; init; }

    /// <summary>Optional unit of measurement.</summary>
    [StringLength(32)]
    public string? Unit { get; init; }

    /// <summary>Attribute data type.</summary>
    public AttributeDataType DataType { get; init; } = AttributeDataType.Text;

    /// <summary>Whether buyers can filter catalog results by this attribute.</summary>
    public bool IsFilterable { get; init; } = true;

    /// <summary>Whether this attribute appears in product comparisons.</summary>
    public bool IsComparable { get; init; } = true;

    /// <summary>Whether sellers must provide this attribute when listing a product.</summary>
    public bool IsRequired { get; init; }

    /// <summary>Predefined selectable values (when DataType = Select).</summary>
    public IReadOnlyList<string> AllowedValues { get; init; } = [];
}

/// <summary>
/// Brand representation in the catalog.
/// </summary>
public sealed record BrandDto
{
    /// <summary>Unique brand identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Brand name.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>URL-friendly brand slug.</summary>
    public string Slug { get; init; } = string.Empty;

    /// <summary>Brand logo URL.</summary>
    public string? LogoUrl { get; init; }
}

/// <summary>
/// Request payload for creating or updating a brand (Admin only).
/// </summary>
public sealed record UpsertBrandRequest
{
    /// <summary>Brand name.</summary>
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public required string Name { get; init; }

    /// <summary>URL-friendly brand slug.</summary>
    [Required]
    [StringLength(120, MinimumLength = 1)]
    public required string Slug { get; init; }

    /// <summary>Brand logo image URL.</summary>
    [Url]
    [StringLength(500)]
    public string? LogoUrl { get; init; }
}
