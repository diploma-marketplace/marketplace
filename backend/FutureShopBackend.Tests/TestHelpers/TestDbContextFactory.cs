using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using FutureShopBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FutureShopBackend.Tests.TestHelpers;

public static class TestDbContextFactory
{
    public static AppDbContext Create(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        var context = new AppDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    public static async Task SeedTestDataAsync(AppDbContext context)
    {
        // Add roles if not seeded
        if (!await context.Roles.AnyAsync())
        {
            context.Roles.AddRange(
                new ApplicationRole { Id = 1, Name = "admin", NormalizedName = "ADMIN" },
                new ApplicationRole { Id = 2, Name = "company", NormalizedName = "COMPANY" },
                new ApplicationRole { Id = 3, Name = "user", NormalizedName = "USER" },
                new ApplicationRole { Id = 4, Name = "guest", NormalizedName = "GUEST" }
            );
        }

        // Add test category
        if (!await context.Categories.AnyAsync(c => c.Id == 1))
        {
            var category = new Category
            {
                Id = 1,
                Name = "Electronics",
                Slug = "electronics"
            };
            category.Attributes.Add(new ProductAttribute { Id = 1, CategoryId = 1, Name = "RAM" });
            category.Attributes.Add(new ProductAttribute { Id = 2, CategoryId = 1, Name = "Storage" });
            context.Categories.Add(category);
        }

        // Add test users
        if (!await context.Users.AnyAsync(u => u.Id == 10))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = 10,
                UserName = "user@test.com",
                Email = "user@test.com",
                PasswordHash = "dummy_hash_10",
                FullName = "Regular User",
                RoleId = 3,
                SellerType = SellerType.Individual,
                VerificationStatus = VerificationStatus.NotSubmitted
            });
        }

        if (!await context.Users.AnyAsync(u => u.Id == 20))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = 20,
                UserName = "fop@test.com",
                Email = "fop@test.com",
                PasswordHash = "dummy_hash_20",
                FullName = "FOP Seller",
                RoleId = 3,
                SellerType = SellerType.FOP,
                VerificationStatus = VerificationStatus.Approved
            });
        }

        if (!await context.Users.AnyAsync(u => u.Id == 30))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = 30,
                UserName = "company@test.com",
                Email = "company@test.com",
                PasswordHash = "dummy_hash_30",
                FullName = "Company Seller",
                RoleId = 2,
                SellerType = SellerType.Company,
                VerificationStatus = VerificationStatus.Approved
            });
        }

        if (!await context.Users.AnyAsync(u => u.Id == 1))
        {
            context.Users.Add(new ApplicationUser
            {
                Id = 1,
                UserName = "admin@test.com",
                Email = "admin@test.com",
                PasswordHash = "dummy_hash_1",
                FullName = "Platform Admin",
                RoleId = 1,
                SellerType = SellerType.Marketplace
            });
        }

        await context.SaveChangesAsync();
    }
}
