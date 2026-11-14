namespace FutureShopBackend.Application.DTO.Common;

/// <summary>
/// Unified error payload returned for all 4xx and 5xx API errors.
/// </summary>
public sealed record ApiErrorResponse
{
    /// <summary>HTTP status code (e.g. 400, 401, 403, 404, 500).</summary>
    /// <example>400</example>
    public int Status { get; init; }

    /// <summary>Machine-readable error code for frontend handling.</summary>
    /// <example>VALIDATION_ERROR</example>
    public string Code { get; init; } = string.Empty;

    /// <summary>Human-readable summary of the error.</summary>
    /// <example>One or more validation errors occurred.</example>
    public string Message { get; init; } = string.Empty;

    /// <summary>Field-level validation errors keyed by property name (present on 400 validation errors).</summary>
    public IReadOnlyDictionary<string, string[]>? Errors { get; init; }

    /// <summary>Distributed trace identifier for server log correlation.</summary>
    /// <example>00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00</example>
    public string? TraceId { get; init; }

    /// <summary>UTC timestamp when the error occurred.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
