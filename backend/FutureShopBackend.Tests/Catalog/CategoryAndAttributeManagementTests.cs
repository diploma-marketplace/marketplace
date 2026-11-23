using FluentAssertions;
using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using FutureShopBackend.Infrastructure.Services;
using FutureShopBackend.Tests.TestHelpers;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace FutureShopBackend.Tests.Catalog;

public class CategoryAndAttributeManagementTests
{
    private readonly Mock<IPhotoUploadService> _photoUploadServiceMock = new();

    [Fact]
    public async Task CreateCategory_WhenAdmin_SucceedsWithAttributes()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(CreateCategory_WhenAdmin_SucceedsWithAttributes));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(1);
        currentUserMock.Setup(c => c.IsAdmin).Returns(true);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        var request = new UpsertCategoryRequest
        {
            Name = "Smartphones",
            Slug = "smartphones",
            Attributes =
            [
                new UpsertCategoryAttributeRequest { Name = "Display Size", DataType = AttributeDataType.Number, Unit = "inches" },
                new UpsertCategoryAttributeRequest { Name = "Battery Capacity", DataType = AttributeDataType.Number, Unit = "mAh" }
            ]
        };

        // Act
        var result = await service.CreateCategoryAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Smartphones");
        result.Slug.Should().Be("smartphones");

        var createdInDb = await db.Categories
            .Include(c => c.Attributes)
            .FirstAsync(c => c.Slug == "smartphones");
        createdInDb.Attributes.Should().HaveCount(2);
        createdInDb.Attributes.Should().Contain(a => a.Name == "Display Size");
        createdInDb.Attributes.Should().Contain(a => a.Name == "Battery Capacity");
    }

    [Fact]
    public async Task AddCategoryAttribute_WhenNonAdmin_ThrowsForbiddenException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(AddCategoryAttribute_WhenNonAdmin_ThrowsForbiddenException));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(10);
        currentUserMock.Setup(c => c.IsAdmin).Returns(false);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        var request = new UpsertCategoryAttributeRequest
        {
            Name = "Screen Refresh Rate",
            DataType = AttributeDataType.Number,
            Unit = "Hz"
        };

        // Act
        var act = () => service.AddCategoryAttributeAsync(1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<ForbiddenException>();
        ex.Which.Code.Should().Be("ADMIN_ROLE_REQUIRED");
    }

    [Fact]
    public async Task AddCategoryAttribute_WhenAdmin_Succeeds()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(AddCategoryAttribute_WhenAdmin_Succeeds));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(1);
        currentUserMock.Setup(c => c.IsAdmin).Returns(true);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        var request = new UpsertCategoryAttributeRequest
        {
            Name = "Processor Model",
            DataType = AttributeDataType.Text
        };

        // Act
        var result = await service.AddCategoryAttributeAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Processor Model");
        result.CategoryId.Should().Be(1);

        var inDb = await db.Attributes.FirstOrDefaultAsync(a => a.CategoryId == 1 && a.Name == "Processor Model");
        inDb.Should().NotBeNull();
    }

    [Fact]
    public async Task AddCategoryAttribute_WhenDuplicateName_ThrowsConflictException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(AddCategoryAttribute_WhenDuplicateName_ThrowsConflictException));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(1);
        currentUserMock.Setup(c => c.IsAdmin).Returns(true);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        // Category 1 already has "RAM"
        var request = new UpsertCategoryAttributeRequest
        {
            Name = "ram", // Case-insensitive duplicate check
            DataType = AttributeDataType.Text
        };

        // Act
        var act = () => service.AddCategoryAttributeAsync(1, request);

        // Assert
        var ex = await act.Should().ThrowAsync<ConflictException>();
        ex.Which.Code.Should().Be("ATTRIBUTE_ALREADY_EXISTS");
    }

    [Fact]
    public async Task UpdateCategoryAttribute_WhenAdmin_Succeeds()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(UpdateCategoryAttribute_WhenAdmin_Succeeds));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(1);
        currentUserMock.Setup(c => c.IsAdmin).Returns(true);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        var request = new UpsertCategoryAttributeRequest
        {
            Name = "System RAM (Memory)",
            DataType = AttributeDataType.Text,
            Unit = "GB"
        };

        // Act
        var result = await service.UpdateCategoryAttributeAsync(1, 1, request);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("System RAM (Memory)");

        var inDb = await db.Attributes.FirstAsync(a => a.Id == 1);
        inDb.Name.Should().Be("System RAM (Memory)");
    }

    [Fact]
    public async Task DeleteCategoryAttribute_WhenAdmin_Succeeds()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(DeleteCategoryAttribute_WhenAdmin_Succeeds));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(1);
        currentUserMock.Setup(c => c.IsAdmin).Returns(true);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        // Act
        await service.DeleteCategoryAttributeAsync(1, 2);

        // Assert
        var inDb = await db.Attributes.FirstOrDefaultAsync(a => a.Id == 2);
        inDb.Should().BeNull();
    }

    [Fact]
    public async Task DeleteCategory_WhenContainsActiveProducts_ThrowsBadRequestException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(DeleteCategory_WhenContainsActiveProducts_ThrowsBadRequestException));
        await TestDbContextFactory.SeedTestDataAsync(db);

        db.Products.Add(new Product
        {
            CategoryId = 1,
            SellerId = 10,
            Title = "Product inside category",
            Price = 1000m,
            IsDeleted = false
        });
        await db.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(1);
        currentUserMock.Setup(c => c.IsAdmin).Returns(true);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        // Act
        var act = () => service.DeleteCategoryAsync(1);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Code.Should().Be("CATEGORY_NOT_EMPTY");
    }
}
