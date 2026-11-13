using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Interaction;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages product and seller reviews, ratings (1-5), verified purchase badges, and seller replies.
/// </summary>
[ApiController]
[Route("api/reviews")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class ReviewsController(IReviewService reviewService) : ControllerBase
{
    /// <summary>
    /// Retrieves paginated reviews for a specific product.
    /// </summary>
    /// <param name="productId">Product identifier.</param>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("product/{productId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<ReviewDto>>> GetProductReviews(
        Guid productId,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var reviews = await reviewService.GetProductReviewsAsync(productId, query, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Retrieves paginated reviews received by a specific seller.
    /// </summary>
    /// <param name="sellerId">Seller identifier.</param>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("seller/{sellerId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<ReviewDto>>> GetSellerReviews(
        Guid sellerId,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var reviews = await reviewService.GetSellerReviewsAsync(sellerId, query, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Retrieves reviews authored by the currently authenticated buyer.
    /// </summary>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("my")]
    [ProducesResponseType(typeof(PagedResponse<ReviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<ReviewDto>>> GetMyWrittenReviews(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var reviews = await reviewService.GetMyWrittenReviewsAsync(query, cancellationToken);
        return Ok(reviews);
    }

    /// <summary>
    /// Submits a new review and star rating (1-5) for a product and seller (User role only).
    /// </summary>
    /// <param name="request">Review rating, pros, cons, and comment.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> CreateReview(
        [FromBody] CreateReviewRequest request,
        CancellationToken cancellationToken)
    {
        var created = await reviewService.CreateReviewAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, created);
    }

    /// <summary>
    /// Posts a seller response to a buyer's review.
    /// </summary>
    /// <param name="reviewId">Review identifier.</param>
    /// <param name="request">Seller reply text.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{reviewId:guid}/reply")]
    [ProducesResponseType(typeof(ReviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewDto>> ReplyToReview(
        Guid reviewId,
        [FromBody] ReplyToReviewRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await reviewService.ReplyToReviewAsync(reviewId, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Deletes a review (accessible to the review author or Admin moderator).
    /// </summary>
    /// <param name="reviewId">Review identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("{reviewId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReview(Guid reviewId, CancellationToken cancellationToken)
    {
        await reviewService.DeleteReviewAsync(reviewId, cancellationToken);
        return NoContent();
    }
}
