using FutureShopBackend.Application.DTO.Commerce;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Sandbox payment processing endpoints for orders (test mode only; no real bank card data is stored).
/// </summary>
[ApiController]
[Route("api/payments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class PaymentsController(IPaymentService paymentService) : ControllerBase
{
    /// <summary>
    /// Initiates a sandbox/test payment session for an order.
    /// </summary>
    /// <param name="request">Target order identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("checkout")]
    [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDetailsResponse>> InitiateTestPayment(
        [FromBody] InitiateTestPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.InitiateTestPaymentAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetPaymentByOrderId), new { orderId = payment.OrderId }, payment);
    }

    /// <summary>
    /// Confirms or simulates the result of a sandbox payment transaction and transitions the order to Paid on success.
    /// </summary>
    /// <param name="paymentId">Payment transaction identifier.</param>
    /// <param name="request">Simulated payment outcome.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{paymentId:guid}/confirm")]
    [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDetailsResponse>> ConfirmTestPayment(
        Guid paymentId,
        [FromBody] ConfirmTestPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.ConfirmTestPaymentAsync(paymentId, request, cancellationToken);
        return Ok(payment);
    }

    /// <summary>
    /// Retrieves payment status and transaction metadata for a specific order.
    /// </summary>
    /// <param name="orderId">Order identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("order/{orderId:guid}")]
    [ProducesResponseType(typeof(PaymentDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PaymentDetailsResponse>> GetPaymentByOrderId(
        Guid orderId,
        CancellationToken cancellationToken)
    {
        var payment = await paymentService.GetPaymentByOrderIdAsync(orderId, cancellationToken);
        return Ok(payment);
    }
}
