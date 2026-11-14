using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Interaction;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages direct messaging conversations between buyers and sellers regarding products and orders.
/// </summary>
[ApiController]
[Route("api/conversations")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class ConversationsController(IConversationService conversationService) : ControllerBase
{
    /// <summary>
    /// Retrieves paginated chat conversations for the authenticated buyer or seller.
    /// </summary>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ConversationSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResponse<ConversationSummaryDto>>> GetMyConversations(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var conversations = await conversationService.GetMyConversationsAsync(query, cancellationToken);
        return Ok(conversations);
    }

    /// <summary>
    /// Starts a new conversation with a seller regarding a product or order.
    /// </summary>
    /// <param name="request">Target seller ID, optional product/order reference, and initial message.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ConversationSummaryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationSummaryDto>> StartConversation(
        [FromBody] StartConversationRequest request,
        CancellationToken cancellationToken)
    {
        var conversation = await conversationService.StartConversationAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, conversation);
    }

    /// <summary>
    /// Retrieves paginated messages within a specific conversation thread.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{conversationId:guid}/messages")]
    [ProducesResponseType(typeof(PagedResponse<ChatMessageDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResponse<ChatMessageDto>>> GetMessages(
        Guid conversationId,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var messages = await conversationService.GetMessagesAsync(conversationId, query, cancellationToken);
        return Ok(messages);
    }

    /// <summary>
    /// Sends a new message inside an existing buyer-seller conversation thread.
    /// </summary>
    /// <param name="conversationId">Conversation identifier.</param>
    /// <param name="request">Message text content.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost("{conversationId:guid}/messages")]
    [ProducesResponseType(typeof(ChatMessageDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatMessageDto>> SendMessage(
        Guid conversationId,
        [FromBody] SendMessageRequest request,
        CancellationToken cancellationToken)
    {
        var message = await conversationService.SendMessageAsync(conversationId, request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, message);
    }
}
