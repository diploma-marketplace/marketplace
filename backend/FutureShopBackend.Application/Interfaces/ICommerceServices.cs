using FutureShopBackend.Application.DTO.Commerce;
using FutureShopBackend.Application.DTO.Common;

namespace FutureShopBackend.Application.Interfaces;

/// <summary>
/// Application service contract for the buyer's shopping cart grouped by sellers.
/// </summary>
public interface ICartService
{
    Task<CartResponse> GetMyCartAsync(CancellationToken cancellationToken = default);
    Task<CartResponse> AddItemAsync(AddCartItemRequest request, CancellationToken cancellationToken = default);
    Task<CartResponse> UpdateItemQuantityAsync(Guid productId, UpdateCartItemQuantityRequest request, CancellationToken cancellationToken = default);
    Task<CartResponse> RemoveItemAsync(Guid productId, CancellationToken cancellationToken = default);
    Task ClearCartAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for managing the buyer's wishlist and price-drop tracking.
/// </summary>
public interface IWishlistService
{
    Task<PagedResponse<WishlistItemDto>> GetMyWishlistAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<WishlistItemDto> AddToWishlistAsync(Guid productId, CancellationToken cancellationToken = default);
    Task RemoveFromWishlistAsync(Guid productId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for order checkout, buyer order history, seller order processing, and status transitions.
/// </summary>
public interface IOrderService
{
    Task<CheckoutOrdersResponse> CreateOrdersFromCartAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderSummaryDto>> GetMyBuyerOrdersAsync(OrderFilterQuery query, CancellationToken cancellationToken = default);
    Task<PagedResponse<OrderSummaryDto>> GetMySellerOrdersAsync(OrderFilterQuery query, CancellationToken cancellationToken = default);
    Task<OrderDetailsResponse> GetOrderByIdAsync(Guid orderId, CancellationToken cancellationToken = default);
    Task<OrderDetailsResponse> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for sandbox/test order payments (no real card details stored).
/// </summary>
public interface IPaymentService
{
    Task<PaymentDetailsResponse> InitiateTestPaymentAsync(InitiateTestPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDetailsResponse> ConfirmTestPaymentAsync(Guid paymentId, ConfirmTestPaymentRequest request, CancellationToken cancellationToken = default);
    Task<PaymentDetailsResponse> GetPaymentByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);
}
