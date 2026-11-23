using FluentAssertions;
using FutureShopBackend.Api.Controllers;
using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FutureShopBackend.Tests.Controllers;

public class ProductsControllerTests
{
    private readonly Mock<IProductService> _productServiceMock = new();
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _controller = new ProductsController(_productServiceMock.Object);
    }

    [Fact]
    public async Task CreateProduct_CallsServiceAndReturnsCreatedAtAction()
    {
        // Arrange
        var request = new UpsertProductRequest
        {
            Title = "Product Title",
            Description = "Product Description with sufficient length",
            CategoryId = 1,
            PriceUah = 100m,
            StockQuantity = 5,
            ImageUrls = ["/uploads/products/1.jpg"]
        };

        var response = new ProductDetailsResponse
        {
            Id = 55,
            Title = request.Title,
            PriceUah = 100m,
            ImageUrls = ["/uploads/products/1.jpg"]
        };

        _productServiceMock
            .Setup(s => s.CreateProductAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actionResult = await _controller.CreateProduct(request, CancellationToken.None);

        // Assert
        var createdResult = actionResult.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdResult.ActionName.Should().Be(nameof(ProductsController.GetProductById));
        createdResult.RouteValues!["productId"].Should().Be(55);
        createdResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task GetProductById_ReturnsOkWithProduct()
    {
        // Arrange
        var response = new ProductDetailsResponse
        {
            Id = 12,
            Title = "Sample Item",
            PriceUah = 250m
        };

        _productServiceMock
            .Setup(s => s.GetProductByIdAsync(12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actionResult = await _controller.GetProductById(12, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task UpdateProduct_ReturnsOkWithUpdatedProduct()
    {
        // Arrange
        var request = new UpsertProductRequest
        {
            Title = "Updated Title",
            Description = "Updated Description with sufficient length",
            CategoryId = 1,
            PriceUah = 300m,
            StockQuantity = 2,
            ImageUrls = ["/uploads/products/updated.jpg"]
        };

        var response = new ProductDetailsResponse
        {
            Id = 12,
            Title = request.Title,
            PriceUah = 300m
        };

        _productServiceMock
            .Setup(s => s.UpdateProductAsync(12, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var actionResult = await _controller.UpdateProduct(12, request, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().Be(response);
    }

    [Fact]
    public async Task DeleteProduct_ReturnsNoContent()
    {
        // Arrange
        _productServiceMock
            .Setup(s => s.DeleteProductAsync(12, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var actionResult = await _controller.DeleteProduct(12, CancellationToken.None);

        // Assert
        actionResult.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task UploadImages_ReturnsOkWithUrls()
    {
        // Arrange
        var filesMock = new Mock<IFormFileCollection>();
        var expectedUrls = new List<string> { "/uploads/products/pic1.jpg", "/uploads/products/pic2.jpg" };

        _productServiceMock
            .Setup(s => s.UploadProductImagesAsync(null, filesMock.Object, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUrls);

        // Act
        var actionResult = await _controller.UploadImages(filesMock.Object, CancellationToken.None);

        // Assert
        var okResult = actionResult.Result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(expectedUrls);
    }
}
