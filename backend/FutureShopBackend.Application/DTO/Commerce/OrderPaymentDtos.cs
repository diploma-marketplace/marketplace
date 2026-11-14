using System.ComponentModel.DataAnnotations;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.DTO.Commerce;

/// <summary>
/// Delivery recipient and shipping address details for checkout.
/// </summary>
public sealed record ShippingDetailsDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public required string RecipientFullName { get; init; }

    [Required]
    [Phone]
    [StringLength(32)]
    public required string RecipientPhone { get; init; }

    [Required]
    public DeliveryMethod DeliveryMethod { get; init; }

    [Required]
    [StringLength(120)]
    public required string City { get; init; }

    /// <summary>Street address or postal branch/locker number.</summary>
    [Required]
    [StringLength(300)]
    public required string AddressOrBranch { get; init; }

    [StringLength(500)]
    public string? Notes { get; init; }
}

/// <summary>
/// Request payload for placing an order from the current shopping cart.
/// </summary>
public sealed record CreateOrderRequest
{
    [Required]
    public required ShippingDetailsDto Shipping { get; init; }

    [Required]
    public PaymentMethod PaymentMethod { get; init; } = PaymentMethod.TestCard;

    /// <summary>
    /// Optional subset of seller IDs from the cart to checkout (if omitted, checks out all seller groups in the cart).
    /// </summary>
    public IReadOnlyList<Guid>? SellerIds { get; init; }
}

/// <summary>
/// Query parameters for filtering buyer or seller orders.
/// </summary>
public sealed record OrderFilterQuery : PaginationQuery
{
    public OrderStatus? Status { get; init; }
}

/// <summary>
/// Single line item in an order.
/// </summary>
public sealed record OrderItemDto
{
    public Guid ProductId { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public string? ProductImageUrl { get; init; }
    public decimal UnitPriceUah { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotalUah => UnitPriceUah * Quantity;
}

/// <summary>
/// Compact order summary used in buyer order history and seller incoming order lists.
/// </summary>
public sealed record OrderSummaryDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid BuyerId { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public DeliveryMethod DeliveryMethod { get; init; }
    public int TotalItemsCount { get; init; }
    public decimal TotalAmountUah { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Full order details including line items, shipping info, payment state, and tracking number.
/// </summary>
public sealed record OrderDetailsResponse
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public Guid BuyerId { get; init; }
    public string BuyerName { get; init; } = string.Empty;
    public Guid SellerId { get; init; }
    public string SellerName { get; init; } = string.Empty;
    public OrderStatus Status { get; init; }
    public PaymentMethod PaymentMethod { get; init; }
    public PaymentStatus PaymentStatus { get; init; }
    public ShippingDetailsDto Shipping { get; init; } = null!;
    public string? TrackingNumber { get; init; }
    public IReadOnlyList<OrderItemDto> Items { get; init; } = [];
    public decimal TotalAmountUah { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }
}

/// <summary>
/// Response returned after checkout when one or more seller-grouped orders are created.
/// </summary>
public sealed record CheckoutOrdersResponse
{
    public IReadOnlyList<OrderDetailsResponse> Orders { get; init; } = [];
    public decimal GrandTotalUah { get; init; }
}

/// <summary>
/// Request payload for transitioning an order through its lifecycle (Created -> Paid -> ConfirmedBySeller -> Shipped -> Delivered -> Completed / Cancelled / Returned).
/// </summary>
public sealed record UpdateOrderStatusRequest
{
    [Required]
    public OrderStatus Status { get; init; }

    /// <summary>Carrier tracking number (optional, used when transitioning to Shipped).</summary>
    [StringLength(100)]
    public string? TrackingNumber { get; init; }

    /// <summary>Reason or comment for status transition (e.g. cancellation or return reason).</summary>
    [StringLength(500)]
    public string? Comment { get; init; }
}

/// <summary>
/// Request payload for initiating a sandbox/test payment session for an order.
/// Real bank card data is never sent to or stored by the platform.
/// </summary>
public sealed record InitiateTestPaymentRequest
{
    [Required]
    public Guid OrderId { get; init; }
}

/// <summary>
/// Request payload for confirming or simulating the outcome of a sandbox payment.
/// </summary>
public sealed record ConfirmTestPaymentRequest
{
    /// <summary>Whether the simulated sandbox payment should succeed or fail.</summary>
    public bool SimulateSuccess { get; init; } = true;

    /// <summary>Optional sandbox payment token (no real PAN/CVV data allowed).</summary>
    [StringLength(128)]
    public string? SandboxToken { get; init; }
}

/// <summary>
/// Sandbox payment status and transaction metadata.
/// </summary>
public sealed record PaymentDetailsResponse
{
    public Guid PaymentId { get; init; }
    public Guid OrderId { get; init; }
    public decimal AmountUah { get; init; }
    public PaymentMethod Method { get; init; }
    public PaymentStatus Status { get; init; }
    public string? SandboxTransactionReference { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? ProcessedAt { get; init; }
}
