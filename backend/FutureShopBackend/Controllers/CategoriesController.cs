using FutureShopBackend.Api.Attributes;
using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Enums;
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
    /// Retrieves a single category by its identifier.
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{categoryId:int}")]
    [ProducesResponseType(typeof(CategoryTreeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryTreeDto>> GetCategoryById(int categoryId, CancellationToken cancellationToken)
    {
        var category = await categoryService.GetCategoryByIdAsync(categoryId, cancellationToken);
        return Ok(category);
    }

    /// <summary>
    /// Retrieves the dynamic specification attributes configured for a category (used for filter panels and product comparisons).
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [HttpGet("{categoryId:int}/attributes")]
    [ProducesResponseType(typeof(IReadOnlyList<CategoryAttributeDefinitionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CategoryAttributeDefinitionDto>>> GetCategoryAttributes(
        int categoryId,
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
    [AuthorizeRoles(UserRole.Admin)]
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
        return CreatedAtAction(nameof(GetCategoryById), new { categoryId = category.Id }, category);
    }

    /// <summary>
    /// Updates an existing category and its dynamic specification attributes (Admin role required).
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="request">Updated category and attribute definitions.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [AuthorizeRoles(UserRole.Admin)]
    [HttpPut("{categoryId:int}")]
    [ProducesResponseType(typeof(CategoryTreeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryTreeDto>> UpdateCategory(
        int categoryId,
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
    [AuthorizeRoles(UserRole.Admin)]
    [HttpDelete("{categoryId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(int categoryId, CancellationToken cancellationToken)
    {
        await categoryService.DeleteCategoryAsync(categoryId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Adds a new dynamic specification attribute to a category (Admin only).
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="request">Attribute definition.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [AuthorizeRoles(UserRole.Admin)]
    [HttpPost("{categoryId:int}/attributes")]
    [ProducesResponseType(typeof(CategoryAttributeDefinitionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryAttributeDefinitionDto>> AddCategoryAttribute(
        int categoryId,
        [FromBody] UpsertCategoryAttributeRequest request,
        CancellationToken cancellationToken)
    {
        var attribute = await categoryService.AddCategoryAttributeAsync(categoryId, request, cancellationToken);
        return CreatedAtAction(nameof(GetCategoryAttributes), new { categoryId }, attribute);
    }

    /// <summary>
    /// Updates an existing specification attribute in a category (Admin only).
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="attributeId">Attribute identifier.</param>
    /// <param name="request">Updated attribute definition.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [AuthorizeRoles(UserRole.Admin)]
    [HttpPut("{categoryId:int}/attributes/{attributeId:int}")]
    [ProducesResponseType(typeof(CategoryAttributeDefinitionDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryAttributeDefinitionDto>> UpdateCategoryAttribute(
        int categoryId,
        int attributeId,
        [FromBody] UpsertCategoryAttributeRequest request,
        CancellationToken cancellationToken)
    {
        var attribute = await categoryService.UpdateCategoryAttributeAsync(categoryId, attributeId, request, cancellationToken);
        return Ok(attribute);
    }

    /// <summary>
    /// Deletes a specification attribute from a category (Admin only).
    /// </summary>
    /// <param name="categoryId">Category identifier.</param>
    /// <param name="attributeId">Attribute identifier.</param>
    /// <param name="cancellationToken">Request cancellation token.</param>
    [AuthorizeRoles(UserRole.Admin)]
    [HttpDelete("{categoryId:int}/attributes/{attributeId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategoryAttribute(
        int categoryId,
        int attributeId,
        CancellationToken cancellationToken)
    {
        await categoryService.DeleteCategoryAttributeAsync(categoryId, attributeId, cancellationToken);
        return NoContent();
    }
}
