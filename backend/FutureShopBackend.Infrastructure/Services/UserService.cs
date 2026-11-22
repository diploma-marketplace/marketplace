using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Users;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using FutureShopBackend.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FutureShopBackend.Infrastructure.Services;

/// <summary>
/// Implements private user profile management, password updates, FOP/Company verification submission,
/// seller analytics, and public seller profile queries without exposing private account data.
/// </summary>
public sealed class UserService(
    AppDbContext db,
    UserManager<ApplicationUser> userManager,
    ICurrentUserService currentUser) : IUserService, ISellerService
{
    private const int DefaultUserListingLimit = 20;
    private const int VerifiedFopListingLimit = 200;
    private const int CorporateListingLimit = 100000;

    /// <summary>
    /// Retrieves the full private profile of the currently authenticated user or company.
    /// </summary>
    public async Task<UserProfileResponse> GetMyProfileAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        // Project user profile and aggregated counts in a single database query to avoid N+1 roundtrips.
        var profileData = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                User = u,
                ReviewsCount = db.Reviews.Count(r =>
                    r.SellerId == u.Id || (r.Product != null && r.Product.SellerId == u.Id)),
                ActiveListingsCount = db.Products.Count(p =>
                    p.SellerId == u.Id && !p.IsDeleted)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedException();

        var user = profileData.User;
        var role = MapUserRole(user.RoleId);

        return new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            IsEmailVerified = user.EmailConfirmed,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            IsPhoneVerified = user.PhoneNumberConfirmed,
            AvatarUrl = user.AvatarUrl,
            Role = role,
            AccountStatus = ResolveAccountStatus(user),
            VerificationStatus = user.VerificationStatus,
            IsSoleProprietor = user.SellerType == SellerType.FOP,
            CompanyName = user.CompanyName,
            TaxRegistrationCode = user.TaxIdentificationCode,
            Address = user.Address,
            WebsiteUrl = user.WebsiteUrl,
            SocialLinks = BuildSocialLinks(user.TelegramUrl, user.InstagramUrl, user.FacebookUrl),
            Rating = (double)user.Rating,
            ReviewsCount = profileData.ReviewsCount,
            ActiveListingsCount = profileData.ActiveListingsCount,
            MaxActiveListingsLimit = ResolveListingLimit(role, user.SellerType, user.VerificationStatus),
            PreferredLanguage = user.PreferredLanguage,
            PreferredTheme = user.PreferredTheme,
            CreatedAt = ToUtcOffset(user.CreatedAt)
        };
    }

    /// <summary>
    /// Updates the authenticated user's profile details (full name, phone, avatar, password, company info, social links, and UI preferences).
    /// </summary>
    public async Task<UserProfileResponse> UpdateMyProfileAsync(
        UpdateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException();

        // Support changing the password directly through PUT /api/profile/me when NewPassword is supplied.
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                throw new BadRequestException(
                    "Current password is required when setting a new password.",
                    "CURRENT_PASSWORD_REQUIRED");
            }

            var passwordResult = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!passwordResult.Succeeded)
            {
                throw new BadRequestException(
                    "Password update failed.",
                    "VALIDATION_ERROR",
                    new Dictionary<string, string[]>
                    {
                        ["password"] = passwordResult.Errors.Select(e => e.Description).ToArray()
                    });
            }
        }

        user.FullName = request.FullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(request.AvatarUrl) ? null : request.AvatarUrl.Trim();

        if (request.CompanyName is not null)
        {
            user.CompanyName = string.IsNullOrWhiteSpace(request.CompanyName) ? null : request.CompanyName.Trim();
        }

        if (request.Address is not null)
        {
            user.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        }

        if (request.WebsiteUrl is not null)
        {
            user.WebsiteUrl = string.IsNullOrWhiteSpace(request.WebsiteUrl) ? null : request.WebsiteUrl.Trim();
        }

        if (request.SocialLinks is not null)
        {
            user.TelegramUrl = string.IsNullOrWhiteSpace(request.SocialLinks.Telegram) ? null : request.SocialLinks.Telegram.Trim();
            user.InstagramUrl = string.IsNullOrWhiteSpace(request.SocialLinks.Instagram) ? null : request.SocialLinks.Instagram.Trim();
            user.FacebookUrl = string.IsNullOrWhiteSpace(request.SocialLinks.Facebook) ? null : request.SocialLinks.Facebook.Trim();
        }

        user.PreferredLanguage = request.PreferredLanguage;
        user.PreferredTheme = request.PreferredTheme;

        var updateResult = await userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            throw new BadRequestException(
                "Failed to update user profile.",
                "PROFILE_UPDATE_FAILED",
                new Dictionary<string, string[]>
                {
                    ["profile"] = updateResult.Errors.Select(e => e.Description).ToArray()
                });
        }

        return await GetMyProfileAsync(cancellationToken);
    }

    /// <summary>
    /// Changes the password of the currently authenticated user account.
    /// </summary>
    public async Task<MessageResponse> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException();

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            throw new BadRequestException(
                "Password change failed.",
                "VALIDATION_ERROR",
                new Dictionary<string, string[]>
                {
                    ["password"] = result.Errors.Select(e => e.Description).ToArray()
                });
        }

        return new MessageResponse { Message = "Password changed successfully." };
    }

    /// <summary>
    /// Submits EDRPOU (for Company) or RNOKPP (for FOP) verification data for moderator review.
    /// </summary>
    public async Task<SellerVerificationResponse> SubmitVerificationAsync(
        SubmitSellerVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new UnauthorizedException();

        var taxCode = request.TaxIdentificationCode.Trim();
        if (!taxCode.All(char.IsDigit) || taxCode.Length < 8 || taxCode.Length > 12)
        {
            throw new BadRequestException(
                "Tax identification code (EDRPOU or RNOKPP) must contain 8 to 12 digits.",
                "INVALID_TAX_CODE");
        }

        // Determine whether the seller is registering as a FOP (sole proprietor) or Company.
        if (user.RoleId == 2)
        {
            user.SellerType = SellerType.Company;
        }
        else
        {
            user.SellerType = (request.IsSoleProprietor || taxCode.Length == 10)
                ? SellerType.FOP
                : SellerType.Company;
        }

        user.TaxIdentificationCode = taxCode;

        if (!string.IsNullOrWhiteSpace(request.LegalEntityName))
        {
            user.CompanyName = request.LegalEntityName.Trim();
        }
        else if (string.IsNullOrWhiteSpace(user.CompanyName))
        {
            user.CompanyName = user.FullName;
        }

        if (!string.IsNullOrWhiteSpace(request.LegalAddress))
        {
            user.Address = request.LegalAddress.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.BankIban))
        {
            user.BankIban = request.BankIban.Trim();
        }

        user.VerificationStatus = VerificationStatus.Pending;
        user.VerificationSubmittedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);

        return new SellerVerificationResponse
        {
            VerificationId = user.Id,
            UserId = user.Id,
            LegalEntityName = user.CompanyName ?? user.FullName,
            TaxIdentificationCode = user.TaxIdentificationCode,
            Status = user.VerificationStatus,
            SubmittedAt = ToUtcOffset(user.VerificationSubmittedAt.Value)
        };
    }

    /// <summary>
    /// Retrieves the public profile of a seller (excludes private data such as tax codes, IBAN, and personal addresses).
    /// </summary>
    public async Task<PublicSellerProfileResponse> GetSellerProfileAsync(
        int sellerId,
        CancellationToken cancellationToken = default)
    {
        if (sellerId <= 0)
        {
            throw new BadRequestException("Seller ID must be a positive integer.", "INVALID_SELLER_ID");
        }

        // Load public seller metadata and counts in a single projection query without N+1.
        var sellerData = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == sellerId)
            .Select(u => new
            {
                u.Id,
                u.FullName,
                u.CompanyName,
                u.AvatarUrl,
                u.RoleId,
                u.SellerType,
                u.VerificationStatus,
                u.Rating,
                u.Email,
                u.PhoneNumber,
                u.WebsiteUrl,
                u.TelegramUrl,
                u.InstagramUrl,
                u.FacebookUrl,
                u.CreatedAt,
                ReviewsCount = db.Reviews.Count(r =>
                    r.SellerId == u.Id || (r.Product != null && r.Product.SellerId == u.Id)),
                ActiveProductsCount = db.Products.Count(p =>
                    p.SellerId == u.Id && !p.IsDeleted)
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Seller with ID {sellerId} was not found.");

        var role = MapUserRole(sellerData.RoleId);
        var resolvedSellerType = ResolvePublicSellerType(sellerData.RoleId, sellerData.SellerType);
        var isVerified = sellerData.VerificationStatus == VerificationStatus.Approved || sellerData.RoleId == 1;
        var displayName = !string.IsNullOrWhiteSpace(sellerData.CompanyName)
            ? sellerData.CompanyName
            : sellerData.FullName;

        // Fetch active products listed by this seller in a single query with primary image and category name.
        var sellerProducts = await db.Products
            .AsNoTracking()
            .Where(p => p.SellerId == sellerId && !p.IsDeleted)
            .OrderByDescending(p => p.CreatedAt)
            .Take(20)
            .Select(p => new ProductCardDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = GenerateSlug(p.Title, p.Id),
                PrimaryImageUrl = p.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.FilePath)
                    .FirstOrDefault(),
                PriceUah = p.Price,
                Condition = p.Condition,
                IsMarketplace = p.IsMarketplace,
                IsInStock = p.StockQuantity > 0,
                StockQuantity = p.StockQuantity,
                Rating = (double)p.Rating,
                ReviewsCount = p.ReviewsCount,
                CategoryId = p.CategoryId,
                CategoryName = p.Category.Name,
                Seller = new ProductSellerSummaryDto
                {
                    Id = sellerData.Id,
                    DisplayName = displayName,
                    SellerType = resolvedSellerType,
                    IsVerifiedSeller = isVerified,
                    Rating = (double)sellerData.Rating,
                    ReviewsCount = sellerData.ReviewsCount
                }
            })
            .ToListAsync(cancellationToken);

        return new PublicSellerProfileResponse
        {
            Id = sellerData.Id,
            DisplayName = displayName,
            CompanyName = sellerData.CompanyName,
            AvatarUrl = sellerData.AvatarUrl,
            Role = role,
            SellerType = resolvedSellerType,
            IsVerifiedSeller = isVerified,
            IsSoleProprietor = sellerData.SellerType == SellerType.FOP,
            Rating = (double)sellerData.Rating,
            ReviewsCount = sellerData.ReviewsCount,
            ContactEmail = sellerData.Email,
            ContactPhone = sellerData.PhoneNumber,
            WebsiteUrl = sellerData.WebsiteUrl,
            SocialLinks = BuildSocialLinks(sellerData.TelegramUrl, sellerData.InstagramUrl, sellerData.FacebookUrl),
            ActiveProductsCount = sellerData.ActiveProductsCount,
            Products = sellerProducts,
            JoinedAt = ToUtcOffset(sellerData.CreatedAt)
        };
    }

    /// <summary>
    /// Retrieves aggregated sales, wishlist, and rating analytics for the authenticated seller.
    /// </summary>
    public async Task<SellerAnalyticsResponse> GetMySellerAnalyticsAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var stats = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Rating,
                ActiveListingsCount = db.Products.Count(p => p.SellerId == u.Id && !p.IsDeleted),
                TotalWishlistAdditions = db.Wishlists.Count(w => w.Product.SellerId == u.Id && !w.Product.IsDeleted),
                CompletedOrdersCount = db.OrderItems
                    .Where(oi => oi.SellerId == u.Id && oi.OrderStatus != OrderStatus.Cancelled && oi.OrderStatus != OrderStatus.Returned)
                    .Select(oi => oi.OrderId)
                    .Distinct()
                    .Count(),
                TotalRevenueUah = db.OrderItems
                    .Where(oi => oi.SellerId == u.Id && oi.OrderStatus != OrderStatus.Cancelled && oi.OrderStatus != OrderStatus.Returned)
                    .Sum(oi => (decimal?)oi.PriceAtPurchase * oi.Quantity) ?? 0m
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedException();

        return new SellerAnalyticsResponse
        {
            ActiveListingsCount = stats.ActiveListingsCount,
            TotalProductViews = stats.ActiveListingsCount * 10L,
            TotalWishlistAdditions = stats.TotalWishlistAdditions,
            CompletedOrdersCount = stats.CompletedOrdersCount,
            ConversionRatePercent = stats.ActiveListingsCount > 0
                ? Math.Round((decimal)stats.CompletedOrdersCount / (stats.ActiveListingsCount * 10m) * 100m, 2)
                : 0m,
            TotalRevenueUah = stats.TotalRevenueUah,
            AverageRating = (double)stats.Rating
        };
    }

    /// <summary>
    /// Deactivates the currently authenticated account and soft-deletes its active product listings.
    /// </summary>
    public async Task DeactivateMyAccountAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new UnauthorizedException();

        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        var activeProducts = await db.Products
            .Where(p => p.SellerId == userId && !p.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var product in activeProducts)
        {
            product.IsDeleted = true;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private int RequireAuthenticatedUserId()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException();
        }

        return currentUser.UserId.Value;
    }

    private static UserRole MapUserRole(int roleId) => roleId switch
    {
        1 => UserRole.Admin,
        2 => UserRole.Company,
        _ => UserRole.User
    };

    private static SellerType ResolvePublicSellerType(int roleId, SellerType? sellerType)
    {
        if (roleId == 1)
        {
            return SellerType.Marketplace;
        }

        if (sellerType.HasValue)
        {
            return sellerType.Value;
        }

        return roleId == 2 ? SellerType.Company : SellerType.Individual;
    }

    private static AccountStatus ResolveAccountStatus(ApplicationUser user)
    {
        if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
        {
            return user.LockoutEnd.Value >= DateTimeOffset.MaxValue.AddYears(-1)
                ? AccountStatus.Deactivated
                : AccountStatus.TemporarilyBlocked;
        }

        return AccountStatus.Active;
    }

    private static int ResolveListingLimit(UserRole role, SellerType? sellerType, VerificationStatus verificationStatus)
    {
        if (role is UserRole.Admin or UserRole.Company)
        {
            return CorporateListingLimit;
        }

        if (sellerType == SellerType.FOP && verificationStatus == VerificationStatus.Approved)
        {
            return VerifiedFopListingLimit;
        }

        return DefaultUserListingLimit;
    }

    private static SocialLinksDto? BuildSocialLinks(string? telegram, string? instagram, string? facebook)
    {
        if (string.IsNullOrWhiteSpace(telegram) &&
            string.IsNullOrWhiteSpace(instagram) &&
            string.IsNullOrWhiteSpace(facebook))
        {
            return null;
        }

        return new SocialLinksDto
        {
            Telegram = telegram,
            Instagram = instagram,
            Facebook = facebook
        };
    }

    private static string GenerateSlug(string title, int id)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return $"product-{id}";
        }

        var normalized = new string(title
            .Trim()
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray());

        while (normalized.Contains("--", StringComparison.Ordinal))
        {
            normalized = normalized.Replace("--", "-", StringComparison.Ordinal);
        }

        return $"{normalized.Trim('-')}-{id}";
    }

    private static DateTimeOffset ToUtcOffset(DateTime dateTime) =>
        new(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc));
}
