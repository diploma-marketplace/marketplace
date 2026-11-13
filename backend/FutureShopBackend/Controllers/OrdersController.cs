using FutureShopBackend.Application.DTO.Commerce;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages order checkout, buyer order history, seller incoming order fulfillment, and order status transitions.
/// </summary>
[ApiController]
[Route("api/orders")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class OrdersController(IOrderService orderService) : ControllerBase
{
    /// <summary>
    /// Places one or more orders (grouped by seller) from the buyer's shopping cart (User role only).
    /// </summary>
    /// <param name="request">Delivery address, delivery method, payment method, and optional subset of seller IDs.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CheckoutOrdersResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CheckoutOrdersResponse>> CreateOrders(
        [FromBody] CreateOrderRequest request,
        CancellationToken cancellationToken)
    {
        var result = await orderService.CreateOrdersFromCartAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>
    /// Retrieves the authenticated buyer's order history.
    /// </summary>
    /// <param name="query">Pagination and order status filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResponse<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<OrderSummaryDto>>> GetMyBuyerOrders(
        [FromQuery] OrderFilterQuery query,
        CancellationToken cancellationToken)
    {
        var orders = await orderService.GetMyBuyerOrdersAsync(query, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves incoming orders placed for products sold by the authenticated seller (User/FOP, Company, or Admin).
    /// </summary>
    /// <param name="query">Pagination and order status filter.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("seller")]
    [ProducesResponseType(typeof(PagedResponse<OrderSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PagedResponse<OrderSummaryDto>>> GetMySellerOrders(
        [FromQuery] OrderFilterQuery query,
        CancellationToken cancellationToken)
    {
        var orders = await orderService.GetMySellerOrdersAsync(query, cancellationToken);
        return Ok(orders);
    }

    /// <summary>
    /// Retrieves full details of a specific order (accessible only to the order's buyer, seller, or Admin).
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailsResponse>> GetOrderById(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var order = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
        return Ok(order);
    }

    /// <summary>
    /// Transitions an order's lifecycle status (e.g. ConfirmedBySeller, Shipped, Delivered, Completed, Cancelled, Returned).
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="request">Target status, optional carrier tracking number, and comment.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPatch("{orderId:guid}/status")]
    [ProducesResponseType(typeof(OrderDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<OrderDetailsResponse>> UpdateOrderStatus(
        Guid orderId,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await orderService.UpdateOrderStatusAsync(orderId, request, cancellationToken);
        return Ok(updated);
    }
}
