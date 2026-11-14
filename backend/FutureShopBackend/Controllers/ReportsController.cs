using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Interaction;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages user complaints and abuse reports targeting products, reviews, or other users.
/// </summary>
[ApiController]
[Route("api/reports")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class ReportsController(IReportService reportService) : ControllerBase
{
    /// <summary>
    /// Submits a complaint against a product listing, review, or user account (available to User, Company, and Admin roles).
    /// </summary>
    /// <param name="request">Target entity type, target ID, and complaint reason.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(ReportDetailsDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReportDetailsDto>> CreateReport(
        [FromBody] CreateReportRequest request,
        CancellationToken cancellationToken)
    {
        var report = await reportService.CreateReportAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, report);
    }
}
