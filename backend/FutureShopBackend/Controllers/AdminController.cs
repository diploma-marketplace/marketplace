using FutureShopBackend.Application.DTO.Admin;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Interaction;
using FutureShopBackend.Application.DTO.Users;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Platform administration endpoints for user management, seller verification, content moderation, settings, analytics, and audit logs (Admin role required).
/// </summary>
[ApiController]
[Route("api/admin")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class AdminController(IAdminService adminService) : ControllerBase
{
    /// <summary>
    /// Retrieves a paginated list of user and company accounts with optional search, role, and status filters.
    /// </summary>
    /// <param name="query">Search, role, status, and pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("users")]
    [ProducesResponseType(typeof(PagedResponse<AdminUserSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<AdminUserSummaryDto>>> GetUsers(
        [FromQuery] AdminUserFilterQuery query,
        CancellationToken cancellationToken)
    {
        var users = await adminService.GetUsersAsync(query, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Blocks, unblocks, or changes the lifecycle status of a user or company account (recorded in audit log).
    /// </summary>
    /// <param name="userId">Target user identifier.</param>
    /// <param name="request">New account status and mandatory reason.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPatch("users/{userId:guid}/status")]
    [ProducesResponseType(typeof(AdminUserSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserSummaryDto>> UpdateUserStatus(
        Guid userId,
        [FromBody] UpdateUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await adminService.UpdateUserStatusAsync(userId, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Changes the primary RBAC role of a user account (recorded in audit log).
    /// </summary>
    /// <param name="userId">Target user identifier.</param>
    /// <param name="request">New role and mandatory reason.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPatch("users/{userId:guid}/role")]
    [ProducesResponseType(typeof(AdminUserSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserSummaryDto>> UpdateUserRole(
        Guid userId,
        [FromBody] UpdateUserRoleRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await adminService.UpdateUserRoleAsync(userId, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Permanently deletes a user account (recorded in audit log).
    /// </summary>
    /// <param name="userId">Target user identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("users/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(Guid userId, CancellationToken cancellationToken)
    {
        await adminService.DeleteUserAsync(userId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Retrieves FOP and Company seller verification applications.
    /// </summary>
    /// <param name="status">Optional verification status filter.</param>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("verifications")]
    [ProducesResponseType(typeof(PagedResponse<SellerVerificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<SellerVerificationResponse>>> GetVerifications(
        [FromQuery] VerificationStatus? status,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var verifications = await adminService.GetVerificationsAsync(status, query, cancellationToken);
        return Ok(verifications);
    }

    /// <summary>
    /// Approves or rejects a FOP or Company seller verification application.
    /// </summary>
    /// <param name="verificationId">Verification request identifier.</param>
    /// <param name="request">Decision status and optional reviewer comment.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPatch("verifications/{verificationId:guid}")]
    [ProducesResponseType(typeof(SellerVerificationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SellerVerificationResponse>> ReviewVerification(
        Guid verificationId,
        [FromBody] ReviewVerificationRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await adminService.ReviewVerificationAsync(verificationId, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Retrieves user complaints and abuse reports for moderation.
    /// </summary>
    /// <param name="status">Optional report status filter.</param>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("reports")]
    [ProducesResponseType(typeof(PagedResponse<ReportDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<ReportDetailsDto>>> GetReports(
        [FromQuery] ReportStatus? status,
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var reports = await adminService.GetReportsAsync(status, query, cancellationToken);
        return Ok(reports);
    }

    /// <summary>
    /// Resolves or dismisses a user complaint/report (recorded in audit log).
    /// </summary>
    /// <param name="reportId">Report identifier.</param>
    /// <param name="request">Resolution status and moderator note.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPatch("reports/{reportId:guid}")]
    [ProducesResponseType(typeof(ReportDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDetailsDto>> ResolveReport(
        Guid reportId,
        [FromBody] ResolveReportRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await adminService.ResolveReportAsync(reportId, request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Retrieves current platform-wide settings (listing limits, commissions, delivery/payment methods).
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlatformSettingsDto>> GetPlatformSettings(CancellationToken cancellationToken)
    {
        var settings = await adminService.GetPlatformSettingsAsync(cancellationToken);
        return Ok(settings);
    }

    /// <summary>
    /// Updates platform-wide settings (listing limits, commissions, delivery/payment methods).
    /// </summary>
    /// <param name="request">Updated platform settings.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPut("settings")]
    [ProducesResponseType(typeof(PlatformSettingsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PlatformSettingsDto>> UpdatePlatformSettings(
        [FromBody] PlatformSettingsDto request,
        CancellationToken cancellationToken)
    {
        var updated = await adminService.UpdatePlatformSettingsAsync(request, cancellationToken);
        return Ok(updated);
    }

    /// <summary>
    /// Retrieves overall platform statistics, GMV, and moderation queue metrics.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(PlatformAnalyticsResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<PlatformAnalyticsResponse>> GetPlatformAnalytics(CancellationToken cancellationToken)
    {
        var analytics = await adminService.GetPlatformAnalyticsAsync(cancellationToken);
        return Ok(analytics);
    }

    /// <summary>
    /// Retrieves the immutable audit log of critical administrative and security actions.
    /// </summary>
    /// <param name="query">Pagination parameters.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("audit-logs")]
    [ProducesResponseType(typeof(PagedResponse<AuditLogEntryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResponse<AuditLogEntryDto>>> GetAuditLogs(
        [FromQuery] PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var logs = await adminService.GetAuditLogsAsync(query, cancellationToken);
        return Ok(logs);
    }
}
