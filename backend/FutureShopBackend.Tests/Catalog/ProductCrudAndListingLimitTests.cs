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

public class ProductCrudAndListingLimitTests
{
    private readonly Mock<IPhotoUploadService> _photoUploadServiceMock = new();

    [Fact]
    public async Task CreateProduct_WithPhotosAndCharacteristics_Succeeds()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(CreateProduct_WithPhotosAndCharacteristics_Succeeds));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(10); // regular user
        currentUserMock.Setup(c => c.IsAdmin).Returns(false);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        var request = new UpsertProductRequest
        {
            Title = "Gaming Laptop RTX 4080",
            Description = "Powerful modern gaming laptop in pristine condition.",
            CategoryId = 1,
            PriceUah = 75000m,
            StockQuantity = 3,
            Condition = ProductCondition.New,
            ImageUrls = ["/uploads/products/laptop1.jpg", "/uploads/products/laptop2.jpg"],
            Specifications =
            [
                new UpsertProductSpecificationRequest { AttributeId = 1, Value = "32 GB" },
                new UpsertProductSpecificationRequest { AttributeId = 2, Value = "1 TB SSD" }
            ]
        };

        // Act
        var result = await service.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Title.Should().Be("Gaming Laptop RTX 4080");
        result.PriceUah.Should().Be(75000m);
        result.ImageUrls.Should().HaveCount(2);
        result.Specifications.Should().HaveCount(2);
        result.Specifications.Should().Contain(s => s.AttributeId == 1 && s.Value == "32 GB");
        result.Specifications.Should().Contain(s => s.AttributeId == 2 && s.Value == "1 TB SSD");
    }

    [Fact]
    public async Task CreateProduct_WhenUserExceedsLimit_ThrowsBadRequestExceptionWithExplanation()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(CreateProduct_WhenUserExceedsLimit_ThrowsBadRequestExceptionWithExplanation));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(10); // regular user (limit 20)
        currentUserMock.Setup(c => c.IsAdmin).Returns(false);

        // Seed 20 active products for this seller
        for (var i = 1; i <= 20; i++)
        {
            db.Products.Add(new Product
            {
                SellerId = 10,
                CategoryId = 1,
                Title = $"Existing Product {i}",
                Price = 1000m,
                StockQuantity = 1,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        var request = new UpsertProductRequest
        {
            Title = "21st Product Beyond Quota",
            Description = "Should be rejected because active listing limit is 20.",
            CategoryId = 1,
            PriceUah = 500m,
            StockQuantity = 1,
            ImageUrls = ["/uploads/products/sample.jpg"]
        };

        // Act
        var act = () => service.CreateProductAsync(request);

        // Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.Which.Code.Should().Be("LISTING_LIMIT_EXCEEDED");
        ex.Which.Message.Should().Contain("20");
        ex.Which.Message.Should().Contain("Active listing limit");
    }

    [Fact]
    public async Task CreateProduct_WhenCompanySeller_AllowsExpandedListingsBeyondUserLimit()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(CreateProduct_WhenCompanySeller_AllowsExpandedListingsBeyondUserLimit));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(30); // Company seller (role 2)
        currentUserMock.Setup(c => c.IsAdmin).Returns(false);

        // Seed 25 products for company seller (more than individual user's 20)
        for (var i = 1; i <= 25; i++)
        {
            db.Products.Add(new Product
            {
                SellerId = 30,
                CategoryId = 1,
                Title = $"Company Product {i}",
                Price = 2000m,
                StockQuantity = 10,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            });
        }
        await db.SaveChangesAsync();

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        var request = new UpsertProductRequest
        {
            Title = "Company Product 26",
            Description = "Company seller can publish listings with expanded quota.",
            CategoryId = 1,
            PriceUah = 1500m,
            StockQuantity = 5,
            ImageUrls = ["/uploads/products/company.jpg"]
        };

        // Act
        var result = await service.CreateProductAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Company Product 26");
    }

    [Fact]
    public async Task UpdateProduct_WhenUserIsNotOwner_ThrowsForbiddenException()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(UpdateProduct_WhenUserIsNotOwner_ThrowsForbiddenException));
        await TestDbContextFactory.SeedTestDataAsync(db);

        // Product owned by User 10
        var product = new Product
        {
            Id = 100,
            SellerId = 10,
            CategoryId = 1,
            Title = "Original Product",
            Price = 1000m,
            StockQuantity = 1,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        // Acting as User 20 (not the owner)
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(20);
        currentUserMock.Setup(c => c.IsAdmin).Returns(false);
        currentUserMock.Setup(c => c.EnsureOwner(10)).Throws(new ForbiddenException("You can access only resources owned by your account.", "RESOURCE_OWNER_REQUIRED"));

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        var request = new UpsertProductRequest
        {
            Title = "Hijacked Title",
            Description = "Malicious update attempt by non-owner.",
            CategoryId = 1,
            PriceUah = 100m,
            StockQuantity = 1,
            ImageUrls = ["/uploads/products/fake.jpg"]
        };

        // Act
        var act = () => service.UpdateProductAsync(100, request);

        // Assert
        var ex = await act.Should().ThrowAsync<ForbiddenException>();
        ex.Which.Code.Should().Be("RESOURCE_OWNER_REQUIRED");
    }

    [Fact]
    public async Task DeleteProduct_DelistsProductSoftDelete()
    {
        // Arrange
        using var db = TestDbContextFactory.Create(nameof(DeleteProduct_DelistsProductSoftDelete));
        await TestDbContextFactory.SeedTestDataAsync(db);

        var product = new Product
        {
            Id = 200,
            SellerId = 10,
            CategoryId = 1,
            Title = "Product to Delist",
            Price = 1500m,
            StockQuantity = 1,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(10);
        currentUserMock.Setup(c => c.IsAdmin).Returns(false);

        var service = new CatalogService(db, currentUserMock.Object, _photoUploadServiceMock.Object);

        // Act
        await service.DeleteProductAsync(200);

        // Assert
        var updatedInDb = await db.Products.FirstAsync(p => p.Id == 200);
        updatedInDb.IsDeleted.Should().BeTrue();
    }
}
