using System.ComponentModel.DataAnnotations;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.DTO.Interaction;

/// <summary>
/// Product and seller review item with verified-purchase indicator and optional seller reply.
/// </summary>
public sealed record ReviewDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public Guid AuthorId { get; init; }
    public string AuthorName { get; init; } = string.Empty;
    public string? AuthorAvatarUrl { get; init; }

    /// <summary>Star rating from 1 to 5.</summary>
    public int Rating { get; init; }

    public string Comment { get; init; } = string.Empty;
    public string? Pros { get; init; }
    public string? Cons { get; init; }

    /// <summary>Indicates whether the author actually purchased the product via Future Market.</summary>
    public bool IsVerifiedPurchase { get; init; }

    /// <summary>Optional response from the seller.</summary>
    public string? SellerReply { get; init; }

    public DateTimeOffset? SellerRepliedAt { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Request payload for submitting a product/seller review (User role only).
/// </summary>
public sealed record CreateReviewRequest
{
    [Required]
    public Guid ProductId { get; init; }

    public Guid? OrderId { get; init; }

    [Required]
    [Range(1, 5)]
    public int Rating { get; init; }

    [Required]
    [StringLength(3000, MinimumLength = 5)]
    public required string Comment { get; init; }

    [StringLength(500)]
    public string? Pros { get; init; }

    [StringLength(500)]
    public string? Cons { get; init; }
}

/// <summary>
/// Request payload for a seller replying to a buyer's review.
/// </summary>
public sealed record ReplyToReviewRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 2)]
    public required string ReplyText { get; init; }
}

/// <summary>
/// Summary of a buyer-seller chat conversation thread.
/// </summary>
public sealed record ConversationSummaryDto
{
    public Guid Id { get; init; }
    public Guid BuyerId { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public Guid? ProductId { get; init; }
    public string? ProductTitle { get; init; }
    public Guid? OrderId { get; init; }
    public string? LastMessagePreview { get; init; }
    public int UnreadCount { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Single message inside a buyer-seller conversation thread.
/// </summary>
public sealed record ChatMessageDto
{
    public Guid Id { get; init; }
    public Guid ConversationId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTimeOffset SentAt { get; init; }
}

/// <summary>
/// Request payload for starting a new conversation with a seller regarding a product or order.
/// </summary>
public sealed record StartConversationRequest
{
    [Required]
    public Guid SellerId { get; init; }

    public Guid? ProductId { get; init; }

    public Guid? OrderId { get; init; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string InitialMessage { get; init; }
}

/// <summary>
/// Request payload for sending a message inside an existing conversation thread.
/// </summary>
public sealed record SendMessageRequest
{
    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string Content { get; init; }
}

/// <summary>
/// Request payload for submitting a complaint/report against a listing, review, or user.
/// </summary>
public sealed record CreateReportRequest
{
    [Required]
    public ReportTargetType TargetType { get; init; }

    [Required]
    public Guid TargetId { get; init; }

    [Required]
    [StringLength(150, MinimumLength = 3)]
    public required string Reason { get; init; }

    [StringLength(2000)]
    public string? Description { get; init; }
}

/// <summary>
/// Detailed representation of a submitted complaint/report.
/// </summary>
public sealed record ReportDetailsDto
{
    public Guid Id { get; init; }
    public Guid ReporterId { get; init; }
    public string ReporterEmail { get; init; } = string.Empty;
    public ReportTargetType TargetType { get; init; }
    public Guid TargetId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ReportStatus Status { get; init; }
    public string? ModeratorNote { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ResolvedAt { get; init; }
}
