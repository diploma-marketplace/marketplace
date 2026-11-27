using FluentAssertions;
using FutureShopBackend.Api.Controllers;
using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FutureShopBackend.Tests.Controllers;

public class CategoriesControllerTests
{
    private readonly Mock<ICategoryService> _categoryServiceMock = new();
    private readonly CategoriesController _controller;

    public CategoriesControllerTests()
    {
        _controller = new CategoriesController(_categoryServiceMock.Object);
    }

    [Fact]
    public async Task GetCategories_ReturnsOkWithTree()
    {
        // Arrange
        var tree = new List<CategoryTreeDto>
        {
            new() { Id = 1, Name = "Computers", Slug = "computers" }
        };

        _categoryServiceMock
            .Setup(s => s.GetCategoryTreeAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tree);

        // Act
        var result = await _controller.GetCategories(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(tree);
    }

    [Fact]
    public async Task CreateCategory_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new UpsertCategoryRequest
        {
            Name = "Monitors",
            Slug = "monitors"
        };

        var response = new CategoryTreeDto
        {
            Id = 5,
            Name = "Monitors",
            Slug = "monitors"
        };

        _categoryServiceMock
            .Setup(s => s.CreateCategoryAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.CreateCategory(request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CategoriesController.GetCategoryById));
        createdResult.RouteValues!["categoryId"].Should().Be(5);
        createdResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task AddCategoryAttribute_ReturnsCreatedAtAction()
    {
        // Arrange
        var request = new UpsertCategoryAttributeRequest
        {
            Name = "Resolution"
        };

        var response = new CategoryAttributeDefinitionDto
        {
            Id = 15,
            CategoryId = 5,
            Name = "Resolution"
        };

        _categoryServiceMock
            .Setup(s => s.AddCategoryAttributeAsync(5, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await _controller.AddCategoryAttribute(5, request, CancellationToken.None);

        // Assert
        var createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(CategoriesController.GetCategoryAttributes));
        createdResult.RouteValues!["categoryId"].Should().Be(5);
        createdResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task DeleteCategory_ReturnsNoContent()
    {
        // Arrange
        _categoryServiceMock
            .Setup(s => s.DeleteCategoryAsync(5, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteCategory(5, CancellationToken.None);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }
}
