using System.ComponentModel.DataAnnotations;

namespace FutureShopBackend.Application.DTO.Common;

/// <summary>
/// Standard paginated response wrapper for list endpoints.
/// </summary>
/// <typeparam name="T">Item DTO type.</typeparam>
public sealed record PagedResponse<T>
{
    /// <summary>Page items.</summary>
    public IReadOnlyList<T> Items { get; init; } = [];

    /// <summary>Current 1-based page number.</summary>
    /// <example>1</example>
    public int Page { get; init; }

    /// <summary>Number of items per page.</summary>
    /// <example>20</example>
    public int PageSize { get; init; }

    /// <summary>Total number of matching items across all pages.</summary>
    /// <example>145</example>
    public int TotalCount { get; init; }

    /// <summary>Total number of pages.</summary>
    public int TotalPages => PageSize > 0 ? (int)Math.Ceiling((double)TotalCount / PageSize) : 0;

    /// <summary>Indicates whether a next page exists.</summary>
    public bool HasNextPage => Page < TotalPages;

    /// <summary>Indicates whether a previous page exists.</summary>
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Standard pagination query parameters.
/// </summary>
public record PaginationQuery
{
    /// <summary>1-based page index.</summary>
    [Range(1, int.MaxValue)]
    public int Page { get; init; } = 1;

    /// <summary>Page size (1..100).</summary>
    [Range(1, 100)]
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Simple confirmation message payload.
/// </summary>
public sealed record MessageResponse
{
    /// <summary>Human-readable operation result message.</summary>
    public string Message { get; init; } = string.Empty;
}
