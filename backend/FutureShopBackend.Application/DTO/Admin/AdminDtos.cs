using System.ComponentModel.DataAnnotations;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.DTO.Admin;

/// <summary>
/// Query parameters for filtering user accounts in the administration panel.
/// </summary>
public sealed record AdminUserFilterQuery : PaginationQuery
{
    [StringLength(150)]
    public string? Search { get; init; }

    public UserRole? Role { get; init; }
    public AccountStatus? Status { get; init; }
}

/// <summary>
/// User account summary returned to platform administrators.
/// </summary>
public sealed record AdminUserSummaryDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public UserRole Role { get; init; }
    public AccountStatus AccountStatus { get; init; }
    public VerificationStatus VerificationStatus { get; init; }
    public int ActiveListingsCount { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Request payload for blocking, unblocking, or changing the status of a user account (Admin only).
/// </summary>
public sealed record UpdateUserStatusRequest
{
    [Required]
    public AccountStatus Status { get; init; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    public required string Reason { get; init; }
}

/// <summary>
/// Request payload for changing a user's primary RBAC role (Admin only).
/// </summary>
public sealed record UpdateUserRoleRequest
{
    [Required]
    public UserRole Role { get; init; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    public required string Reason { get; init; }
}

/// <summary>
/// Request payload for approving or rejecting a FOP/Company seller verification request (Admin only).
/// </summary>
public sealed record ReviewVerificationRequest
{
    [Required]
    public VerificationStatus Status { get; init; }

    [StringLength(500)]
    public string? ReviewerComment { get; init; }
}

/// <summary>
/// Request payload for resolving or dismissing a user complaint/report (Admin only).
/// </summary>
public sealed record ResolveReportRequest
{
    [Required]
    public ReportStatus Status { get; init; }

    [Required]
    [StringLength(500, MinimumLength = 3)]
    public required string ModeratorNote { get; init; }
}

/// <summary>
/// Configurable platform-wide settings (listing limits, commissions, enabled payment/delivery methods).
/// </summary>
public sealed record PlatformSettingsDto
{
    /// <summary>Maximum active listings allowed for unverified individual sellers.</summary>
    [Range(1, 10000)]
    public int DefaultUserActiveListingsLimit { get; init; } = 20;

    /// <summary>Maximum active listings allowed for verified FOP sellers.</summary>
    [Range(1, 100000)]
    public int VerifiedFopActiveListingsLimit { get; init; } = 200;

    /// <summary>Platform commission percentage on third-party marketplace sales.</summary>
    [Range(0, 100)]
    public decimal CommissionPercentage { get; init; } = 5.0m;

    public IReadOnlyList<DeliveryMethod> EnabledDeliveryMethods { get; init; } = [];
    public IReadOnlyList<PaymentMethod> EnabledPaymentMethods { get; init; } = [];
}

/// <summary>
/// Platform-wide analytics and financial metrics for administrators.
/// </summary>
public sealed record PlatformAnalyticsResponse
{
    public int TotalUsersCount { get; init; }
    public int TotalVerifiedCompaniesCount { get; init; }
    public int TotalActiveProductsCount { get; init; }
    public int TotalOrdersCount { get; init; }
    public decimal GrossMerchandiseVolumeUah { get; init; }
    public decimal PlatformCommissionRevenueUah { get; init; }
    public int PendingVerificationsCount { get; init; }
    public int PendingReportsCount { get; init; }
}

/// <summary>
/// Immutable audit log entry recording critical administrative and security actions.
/// </summary>
public sealed record AuditLogEntryDto
{
    public Guid Id { get; init; }
    public Guid ActorUserId { get; init; }
    public string ActorEmail { get; init; } = string.Empty;
    public string Action { get; init; } = string.Empty;
    public string TargetEntityType { get; init; } = string.Empty;
    public string TargetEntityId { get; init; } = string.Empty;
    public string? Details { get; init; }
    public DateTimeOffset OccurredAt { get; init; }
}
