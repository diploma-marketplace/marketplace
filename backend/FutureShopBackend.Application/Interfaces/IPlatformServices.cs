using FutureShopBackend.Application.DTO.Admin;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Interaction;
using FutureShopBackend.Application.DTO.Users;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.Interfaces;

/// <summary>
/// Application service contract for product and seller reviews and ratings.
/// </summary>
public interface IReviewService
{
    Task<PagedResponse<ReviewDto>> GetProductReviewsAsync(Guid productId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<PagedResponse<ReviewDto>> GetSellerReviewsAsync(Guid sellerId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<PagedResponse<ReviewDto>> GetMyWrittenReviewsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ReviewDto> CreateReviewAsync(CreateReviewRequest request, CancellationToken cancellationToken = default);
    Task<ReviewDto> ReplyToReviewAsync(Guid reviewId, ReplyToReviewRequest request, CancellationToken cancellationToken = default);
    Task DeleteReviewAsync(Guid reviewId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for buyer-seller direct messaging conversations.
/// </summary>
public interface IConversationService
{
    Task<PagedResponse<ConversationSummaryDto>> GetMyConversationsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ConversationSummaryDto> StartConversationAsync(StartConversationRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<ChatMessageDto>> GetMessagesAsync(Guid conversationId, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ChatMessageDto> SendMessageAsync(Guid conversationId, SendMessageRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for submitting user complaints/reports.
/// </summary>
public interface IReportService
{
    Task<ReportDetailsDto> CreateReportAsync(CreateReportRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for platform administration, user management, moderation, settings, analytics, and audit logging.
/// </summary>
public interface IAdminService
{
    Task<PagedResponse<AdminUserSummaryDto>> GetUsersAsync(AdminUserFilterQuery query, CancellationToken cancellationToken = default);
    Task<AdminUserSummaryDto> UpdateUserStatusAsync(Guid userId, UpdateUserStatusRequest request, CancellationToken cancellationToken = default);
    Task<AdminUserSummaryDto> UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<PagedResponse<SellerVerificationResponse>> GetVerificationsAsync(VerificationStatus? status, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<SellerVerificationResponse> ReviewVerificationAsync(Guid verificationId, ReviewVerificationRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<ReportDetailsDto>> GetReportsAsync(ReportStatus? status, PaginationQuery query, CancellationToken cancellationToken = default);
    Task<ReportDetailsDto> ResolveReportAsync(Guid reportId, ResolveReportRequest request, CancellationToken cancellationToken = default);
    Task<PlatformSettingsDto> GetPlatformSettingsAsync(CancellationToken cancellationToken = default);
    Task<PlatformSettingsDto> UpdatePlatformSettingsAsync(PlatformSettingsDto request, CancellationToken cancellationToken = default);
    Task<PlatformAnalyticsResponse> GetPlatformAnalyticsAsync(CancellationToken cancellationToken = default);
    Task<PagedResponse<AuditLogEntryDto>> GetAuditLogsAsync(PaginationQuery query, CancellationToken cancellationToken = default);
}
