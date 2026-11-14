using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FutureShopBackend.Api.Controllers;

/// <summary>
/// Manages hierarchical product categories and dynamic specification attributes used for catalog filtering and comparison.
/// </summary>
[ApiController]
[Route("api/categories")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status500InternalServerError)]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    /// <summary>
    /// Retrieves the full hierarchical category tree.
    /// </summary>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryTreeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<CategoryTreeDto>>> GetCategories(CancellationToken cancellationToken)
    {
        var tree = await categoryService.GetCategoryTreeAsync(cancellationToken);
        return Ok(tree);
    }

    /// <summary>
    /// Retrieves the dynamic specification attributes configured for a category (used for filter panels and product comparisons).
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{categoryId:guid}/attributes")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryAttributeDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CategoryAttributeDefinitionDto>>> GetCategoryAttributes(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var attributes = await categoryService.GetCategoryAttributesAsync(categoryId, cancellationToken);
        return Ok(attributes);
    }

    /// <summary>
    /// Creates a new category and its dynamic specification attributes (Admin role required).
    /// </summary>
    /// <param name="request">Category and attribute definitions.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryTreeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CategoryTreeDto>> CreateCategory(
        [FromBody] UpsertCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
    }

    /// <summary>
    /// Updates an existing category and its dynamic specification attributes (Admin role required).
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="request">Updated category and attribute definitions.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpPut("{categoryId:guid}")]
    [ProducesResponseType(typeof(CategoryTreeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryTreeDto>> UpdateCategory(
        Guid categoryId,
        [FromBody] UpsertCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = await categoryService.UpdateCategoryAsync(categoryId, request, cancellationToken);
        return Ok(category);
    }

    /// <summary>
    /// Deletes a category (Admin role required).
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpDelete("{categoryId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(Guid categoryId, CancellationToken cancellationToken)
    {
        await categoryService.DeleteCategoryAsync(categoryId, cancellationToken);
        return NoContent();
    }
}
