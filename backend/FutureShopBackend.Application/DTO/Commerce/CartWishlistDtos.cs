using System.ComponentModel.DataAnnotations;
using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.DTO.Commerce;

/// <summary>
/// Single line item inside a seller's cart group.
/// </summary>
public sealed record CartItemDto
{
    public Guid ProductId { get; init; }
    public string ProductTitle { get; init; } = string.Empty;
    public string ProductSlug { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public decimal UnitPriceUah { get; init; }
    public int Quantity { get; init; }
    public int AvailableStock { get; init; }
    public decimal LineTotalUah => UnitPriceUah * Quantity;
}

/// <summary>
/// Group of cart items sold by a specific seller with calculated group subtotal.
/// </summary>
public sealed record CartSellerGroupDto
{
    public Guid SellerId { get; init; }
    public string SellerDisplayName { get; init; } = string.Empty;
    public SellerType SellerType { get; init; }
    public bool IsVerifiedSeller { get; init; }
    public IReadOnlyList<CartItemDto> Items { get; init; } = [];
    public decimal GroupSubtotalUah { get; init; }
}

/// <summary>
/// Full shopping cart state grouped by sellers with automatic total cost calculation.
/// </summary>
public sealed record CartResponse
{
    public IReadOnlyList<CartSellerGroupDto> SellerGroups { get; init; } = [];
    public int TotalItemsCount { get; init; }
    public decimal TotalAmountUah { get; init; }
}

/// <summary>
/// Request payload for adding a product to the shopping cart.
/// </summary>
public sealed record AddCartItemRequest
{
    [Required]
    public Guid ProductId { get; init; }

    [Required]
    [Range(1, 1000)]
    public int Quantity { get; init; } = 1;
}

/// <summary>
/// Request payload for updating the quantity of a product in the shopping cart.
/// </summary>
public sealed record UpdateCartItemQuantityRequest
{
    [Required]
    [Range(1, 1000)]
    public int Quantity { get; init; }
}

/// <summary>
/// Wishlist item tracking price changes since the product was saved by the user.
/// </summary>
public sealed record WishlistItemDto
{
    public Guid ProductId { get; init; }
    public ProductCardDto Product { get; init; } = new();

    /// <summary>Product price in UAH at the moment it was added to the wishlist.</summary>
    public decimal PriceAtAdditionUah { get; init; }

    /// <summary>Current product price in UAH.</summary>
    public decimal CurrentPriceUah { get; init; }

    /// <summary>Signed price difference (CurrentPriceUah - PriceAtAdditionUah).</summary>
    public decimal PriceDifferenceUah => CurrentPriceUah - PriceAtAdditionUah;

    /// <summary>Indicates whether the current price is lower than when added to the wishlist.</summary>
    public bool IsPriceDropped => CurrentPriceUah < PriceAtAdditionUah;

    public DateTimeOffset AddedAt { get; init; }
}
