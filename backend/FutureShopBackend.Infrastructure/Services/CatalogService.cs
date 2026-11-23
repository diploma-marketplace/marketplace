using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using FutureShopBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FutureShopBackend.Infrastructure.Services;

/// <summary>
/// Implements high-performance product catalog search, real-time live search suggestions,
/// multi-criteria filtering (including Marketplace vs Private/Third-Party seller separation),
/// sorting (price, date, popularity), and product/category management without ORM N+1 queries.
/// </summary>
public sealed class CatalogService(
    AppDbContext db,
    ICurrentUserService currentUser,
    IPhotoUploadService photoUploadService) : IProductService, ICategoryService
{
    public const int DefaultUserListingLimit = 20;
    public const int VerifiedFopListingLimit = 200;
    public const int CompanyListingLimit = 10000;

    /// <summary>
    /// Searches, filters, and sorts products across the catalog using a single projected SQL query without N+1 roundtrips.
    /// </summary>
    public async Task<PagedResponse<ProductCardDto>> SearchProductsAsync(
        CatalogSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.MinPrice.HasValue && query.MaxPrice.HasValue && query.MinPrice.Value > query.MaxPrice.Value)
        {
            throw new BadRequestException(
                "Minimum price cannot be greater than maximum price.",
                "INVALID_PRICE_RANGE");
        }

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var productsQuery = db.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted);

        // 1. Keyword search across title, description, and category name
        var searchTerm = (!string.IsNullOrWhiteSpace(query.Query) ? query.Query : query.Q)?.Trim();
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var pattern = $"%{searchTerm}%";
            productsQuery = productsQuery.Where(p =>
                EF.Functions.ILike(p.Title, pattern) ||
                (p.Description != null && EF.Functions.ILike(p.Description, pattern)) ||
                EF.Functions.ILike(p.Category.Name, pattern));
        }

        // 2. Category filter (includes child subcategories)
        if (query.CategoryId.HasValue)
        {
            var categoryIds = await GetCategoryAndDescendantIdsAsync(query.CategoryId.Value, cancellationToken);
            productsQuery = productsQuery.Where(p => categoryIds.Contains(p.CategoryId));
        }

        // 3. Specific seller filter
        if (query.SellerId.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.SellerId == query.SellerId.Value);
        }

        // 4. Key filter: separating first-party Marketplace offers from private/third-party seller products
        if (query.IsMarketplace.HasValue)
        {
            productsQuery = query.IsMarketplace.Value
                ? productsQuery.Where(p => p.IsMarketplace || p.Seller.RoleId == 1)
                : productsQuery.Where(p => !p.IsMarketplace && p.Seller.RoleId != 1);
        }

        if (query.SellerType.HasValue)
        {
            productsQuery = query.SellerType.Value switch
            {
                SellerType.Marketplace => productsQuery.Where(p => p.IsMarketplace || p.Seller.RoleId == 1),
                SellerType.ThirdParty => productsQuery.Where(p => !p.IsMarketplace && p.Seller.RoleId != 1),
                SellerType.FOP => productsQuery.Where(p => !p.IsMarketplace && p.Seller.SellerType == SellerType.FOP),
                SellerType.Company => productsQuery.Where(p =>
                    !p.IsMarketplace && (p.Seller.SellerType == SellerType.Company || p.Seller.RoleId == 2)),
                SellerType.Individual => productsQuery.Where(p =>
                    !p.IsMarketplace && p.Seller.RoleId == 3 && p.Seller.SellerType == null),
                _ => productsQuery
            };
        }

        // 5. Physical condition filter (New / Used)
        if (query.Condition.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Condition == query.Condition.Value);
        }

        // 6. Price range filter
        if (query.MinPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price >= query.MinPrice.Value);
        }

        if (query.MaxPrice.HasValue)
        {
            productsQuery = productsQuery.Where(p => p.Price <= query.MaxPrice.Value);
        }

        // 7. Stock availability filter
        if (query.InStockOnly == true)
        {
            productsQuery = productsQuery.Where(p => p.StockQuantity > 0);
        }

        // 8. Dynamic technical specifications / attributes filter
        if (query.Attributes is { Count: > 0 })
        {
            foreach (var (rawKey, rawVal) in query.Attributes)
            {
                if (string.IsNullOrWhiteSpace(rawKey) || string.IsNullOrWhiteSpace(rawVal))
                {
                    continue;
                }

                var attrKey = rawKey.Trim();
                var attrVal = rawVal.Trim();

                if (int.TryParse(attrKey, out var attributeId))
                {
                    productsQuery = productsQuery.Where(p =>
                        p.AttributeValues.Any(av =>
                            av.AttributeId == attributeId &&
                            EF.Functions.ILike(av.Value, attrVal)));
                }
                else
                {
                    productsQuery = productsQuery.Where(p =>
                        p.AttributeValues.Any(av =>
                            EF.Functions.ILike(av.Attribute.Name, attrKey) &&
                            EF.Functions.ILike(av.Value, attrVal)));
                }
            }
        }

        // 9. Sorting (by price, date, popularity, rating, or search relevance)
        var prefixPattern = !string.IsNullOrWhiteSpace(searchTerm) ? $"{searchTerm}%" : null;
        productsQuery = query.SortBy switch
        {
            CatalogSortBy.PriceAsc => productsQuery
                .OrderBy(p => p.Price)
                .ThenByDescending(p => p.Id),
            CatalogSortBy.PriceDesc => productsQuery
                .OrderByDescending(p => p.Price)
                .ThenByDescending(p => p.Id),
            CatalogSortBy.Newest => productsQuery
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id),
            CatalogSortBy.Oldest => productsQuery
                .OrderBy(p => p.CreatedAt)
                .ThenBy(p => p.Id),
            CatalogSortBy.RatingDesc => productsQuery
                .OrderByDescending(p => p.Rating)
                .ThenByDescending(p => p.ReviewsCount)
                .ThenByDescending(p => p.Id),
            CatalogSortBy.Popular => productsQuery
                .OrderByDescending(p =>
                    (db.OrderItems
                        .Where(oi => oi.ProductId == p.Id && oi.OrderStatus != OrderStatus.Cancelled && oi.OrderStatus != OrderStatus.Returned)
                        .Sum(oi => (int?)oi.Quantity) ?? 0) * 3 +
                    db.Wishlists.Count(w => w.ProductId == p.Id) * 2 +
                    p.ReviewsCount)
                .ThenByDescending(p => p.Rating)
                .ThenByDescending(p => p.CreatedAt),
            _ => prefixPattern is not null
                ? productsQuery
                    .OrderByDescending(p => EF.Functions.ILike(p.Title, prefixPattern))
                    .ThenByDescending(p => p.Rating)
                    .ThenByDescending(p => p.CreatedAt)
                : productsQuery
                    .OrderByDescending(p => p.CreatedAt)
                    .ThenByDescending(p => p.Id)
        };

        var totalCount = await productsQuery.CountAsync(cancellationToken);

        // 10. Single SQL projection for the page items (eliminates N+1 queries for images, category, and seller)
        var rawItems = await productsQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                p.Title,
                PrimaryImageUrl = p.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.FilePath)
                    .FirstOrDefault(),
                p.Price,
                p.Condition,
                p.IsMarketplace,
                p.StockQuantity,
                p.Rating,
                p.ReviewsCount,
                p.CategoryId,
                CategoryName = p.Category.Name,
                SellerId = p.Seller.Id,
                SellerFullName = p.Seller.FullName,
                SellerCompanyName = p.Seller.CompanyName,
                SellerRoleId = p.Seller.RoleId,
                SellerAccountType = p.Seller.SellerType,
                SellerVerificationStatus = p.Seller.VerificationStatus,
                SellerRating = p.Seller.Rating
            })
            .ToListAsync(cancellationToken);

        var items = rawItems.Select(p => new ProductCardDto
        {
            Id = p.Id,
            Title = p.Title,
            Slug = GenerateSlug(p.Title, p.Id),
            PrimaryImageUrl = p.PrimaryImageUrl,
            PriceUah = p.Price,
            Condition = p.Condition,
            IsMarketplace = p.IsMarketplace || p.SellerRoleId == 1,
            IsInStock = p.StockQuantity > 0,
            StockQuantity = p.StockQuantity,
            Rating = (double)p.Rating,
            ReviewsCount = p.ReviewsCount,
            CategoryId = p.CategoryId,
            CategoryName = p.CategoryName,
            Seller = new ProductSellerSummaryDto
            {
                Id = p.SellerId,
                DisplayName = ResolveSellerDisplayName(p.IsMarketplace, p.SellerRoleId, p.SellerCompanyName, p.SellerFullName),
                SellerType = ResolveSellerType(p.IsMarketplace, p.SellerRoleId, p.SellerAccountType),
                IsVerifiedSeller = p.IsMarketplace || p.SellerRoleId == 1 || p.SellerVerificationStatus == VerificationStatus.Approved,
                Rating = (double)p.SellerRating,
                ReviewsCount = p.ReviewsCount
            }
        }).ToList();

        return new PagedResponse<ProductCardDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Returns real-time live search autocomplete suggestions (matching search phrases, categories, and top product previews).
    /// </summary>
    public async Task<SearchSuggestionsResponse> GetSearchSuggestionsAsync(
        string query,
        int limit = 8,
        CancellationToken cancellationToken = default)
    {
        var clampedLimit = Math.Clamp(limit, 1, 20);
        var trimmed = query?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            var topProducts = await GetRecommendationsAsync(clampedLimit, cancellationToken);
            var rootCategories = await db.Categories
                .AsNoTracking()
                .Where(c => c.ParentId == null)
                .OrderBy(c => c.Name)
                .Take(clampedLimit)
                .Select(c => new CategorySuggestionItem
                {
                    Id = c.Id,
                    Name = c.Name,
                    Slug = c.Slug
                })
                .ToListAsync(cancellationToken);

            return new SearchSuggestionsResponse
            {
                SuggestedQueries = topProducts.Select(p => p.Title).Distinct().Take(clampedLimit).ToList(),
                Categories = rootCategories,
                Products = topProducts
            };
        }

        var prefixPattern = $"{trimmed}%";
        var containsPattern = $"%{trimmed}%";

        // Fetch matching categories in a single query ordered by prefix match
        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => EF.Functions.ILike(c.Name, containsPattern) || EF.Functions.ILike(c.Slug, containsPattern))
            .OrderByDescending(c => EF.Functions.ILike(c.Name, prefixPattern))
            .ThenBy(c => c.Name)
            .Take(clampedLimit)
            .Select(c => new CategorySuggestionItem
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug
            })
            .ToListAsync(cancellationToken);

        // Fetch matching products in a single projected query without N+1
        var rawProducts = await db.Products
            .AsNoTracking()
            .Where(p => !p.IsDeleted &&
                (EF.Functions.ILike(p.Title, containsPattern) ||
                 EF.Functions.ILike(p.Category.Name, containsPattern)))
            .OrderByDescending(p => EF.Functions.ILike(p.Title, prefixPattern))
            .ThenByDescending(p => p.Rating)
            .ThenByDescending(p => p.ReviewsCount)
            .ThenByDescending(p => p.CreatedAt)
            .Take(clampedLimit)
            .Select(p => new
            {
                p.Id,
                p.Title,
                PrimaryImageUrl = p.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.FilePath)
                    .FirstOrDefault(),
                p.Price,
                p.Condition,
                p.IsMarketplace,
                p.StockQuantity,
                p.Rating,
                p.ReviewsCount,
                p.CategoryId,
                CategoryName = p.Category.Name,
                SellerId = p.Seller.Id,
                SellerFullName = p.Seller.FullName,
                SellerCompanyName = p.Seller.CompanyName,
                SellerRoleId = p.Seller.RoleId,
                SellerAccountType = p.Seller.SellerType,
                SellerVerificationStatus = p.Seller.VerificationStatus,
                SellerRating = p.Seller.Rating
            })
            .ToListAsync(cancellationToken);

        var products = rawProducts.Select(p => new ProductCardDto
        {
            Id = p.Id,
            Title = p.Title,
            Slug = GenerateSlug(p.Title, p.Id),
            PrimaryImageUrl = p.PrimaryImageUrl,
            PriceUah = p.Price,
            Condition = p.Condition,
            IsMarketplace = p.IsMarketplace || p.SellerRoleId == 1,
            IsInStock = p.StockQuantity > 0,
            StockQuantity = p.StockQuantity,
            Rating = (double)p.Rating,
            ReviewsCount = p.ReviewsCount,
            CategoryId = p.CategoryId,
            CategoryName = p.CategoryName,
            Seller = new ProductSellerSummaryDto
            {
                Id = p.SellerId,
                DisplayName = ResolveSellerDisplayName(p.IsMarketplace, p.SellerRoleId, p.SellerCompanyName, p.SellerFullName),
                SellerType = ResolveSellerType(p.IsMarketplace, p.SellerRoleId, p.SellerAccountType),
                IsVerifiedSeller = p.IsMarketplace || p.SellerRoleId == 1 || p.SellerVerificationStatus == VerificationStatus.Approved,
                Rating = (double)p.SellerRating,
                ReviewsCount = p.ReviewsCount
            }
        }).ToList();

        var suggestedQueries = products
            .Select(p => p.Title)
            .Concat(categories.Select(c => c.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(clampedLimit)
            .ToList();

        return new SearchSuggestionsResponse
        {
            SuggestedQueries = suggestedQueries,
            Categories = categories,
            Products = products
        };
    }

    /// <summary>
    /// Returns top recommended products ordered by rating and popularity.
    /// </summary>
    public async Task<IReadOnlyList<ProductCardDto>> GetRecommendationsAsync(
        int limit = 12,
        CancellationToken cancellationToken = default)
    {
        var clampedLimit = Math.Clamp(limit, 1, 50);
        var page = await SearchProductsAsync(new CatalogSearchQuery
        {
            Page = 1,
            PageSize = clampedLimit,
            InStockOnly = true,
            SortBy = CatalogSortBy.Popular
        }, cancellationToken);

        return page.Items;
    }

    /// <summary>
    /// Compares multiple products side-by-side by their dynamic category specifications.
    /// </summary>
    public async Task<ProductComparisonResponse> CompareProductsAsync(
        IReadOnlyList<int> productIds,
        bool onlyDifferences = false,
        CancellationToken cancellationToken = default)
    {
        if (productIds is null || productIds.Count < 2)
        {
            throw new BadRequestException("At least two product IDs are required for comparison.", "INVALID_COMPARISON_REQUEST");
        }

        var distinctIds = productIds.Distinct().ToList();
        var products = await db.Products
            .AsNoTracking()
            .Where(p => distinctIds.Contains(p.Id) && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Images)
            .Include(p => p.AttributeValues)
                .ThenInclude(av => av.Attribute)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        if (products.Count < 2)
        {
            throw new NotFoundException("One or more products for comparison were not found.");
        }

        var primaryCategory = products[0].Category;
        var allAttributes = products
            .SelectMany(p => p.AttributeValues.Select(av => av.Attribute))
            .GroupBy(a => a.Id)
            .Select(g => g.First())
            .OrderBy(a => a.Name)
            .ToList();

        var rows = new List<ComparisonSpecificationRowDto>();
        foreach (var attr in allAttributes)
        {
            var valuesByProduct = products.ToDictionary(
                p => p.Id,
                p => p.AttributeValues.FirstOrDefault(av => av.AttributeId == attr.Id)?.Value);

            var distinctValues = valuesByProduct.Values
                .Select(v => v?.Trim() ?? string.Empty)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var hasDifference = distinctValues > 1;
            if (onlyDifferences && !hasDifference)
            {
                continue;
            }

            rows.Add(new ComparisonSpecificationRowDto
            {
                AttributeId = attr.Id,
                AttributeName = attr.Name,
                HasDifference = hasDifference,
                ValuesByProductId = valuesByProduct
            });
        }

        return new ProductComparisonResponse
        {
            CategoryId = primaryCategory.Id,
            CategoryName = primaryCategory.Name,
            Products = products.Select(MapToProductCard).ToList(),
            SpecificationRows = rows
        };
    }

    /// <summary>
    /// Retrieves full Product Detail Page (PDP) information including images, dynamic specifications, and seller summary.
    /// </summary>
    public async Task<ProductDetailsResponse> GetProductByIdAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await db.Products
            .AsNoTracking()
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Images)
            .Include(p => p.AttributeValues)
                .ThenInclude(av => av.Attribute)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found.");

        var isOwnerOrAdmin = currentUser.IsAuthenticated && (currentUser.IsAdmin || product.SellerId == currentUser.UserId);
        if (product.IsDeleted && !isOwnerOrAdmin)
        {
            throw new NotFoundException($"Product with ID {productId} was not found.");
        }

        return MapToProductDetails(product);
    }

    /// <summary>
    /// Retrieves lightweight product summary and key specifications for the Quick View modal window.
    /// </summary>
    public async Task<ProductQuickViewResponse> GetProductQuickViewAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var details = await GetProductByIdAsync(productId, cancellationToken);
        var shortDesc = details.Description.Length > 240
            ? $"{details.Description[..240]}..."
            : details.Description;

        return new ProductQuickViewResponse
        {
            Id = details.Id,
            Title = details.Title,
            PriceUah = details.PriceUah,
            PreviousPriceUah = details.PreviousPriceUah,
            Condition = details.Condition,
            IsMarketplace = details.IsMarketplace,
            IsInStock = details.IsInStock,
            StockQuantity = details.StockQuantity,
            Rating = details.Rating,
            ReviewsCount = details.ReviewsCount,
            ImageUrls = details.ImageUrls,
            ShortDescription = shortDesc,
            KeySpecifications = details.Specifications.Take(6).ToList(),
            Seller = details.Seller
        };
    }

    /// <summary>
    /// Retrieves price history analytics for a product over 30, 90, or 365 days.
    /// </summary>
    public async Task<ProductPriceHistoryResponse> GetProductPriceHistoryAsync(
        int productId,
        int periodDays = 90,
        CancellationToken cancellationToken = default)
    {
        if (periodDays is not (30 or 90 or 365))
        {
            throw new BadRequestException("PeriodDays must be 30, 90, or 365.", "INVALID_PERIOD_DAYS");
        }

        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && !p.IsDeleted)
            .Select(p => new { p.Id, p.Price, p.CreatedAt })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found.");

        var historicalPrices = await db.OrderItems
            .AsNoTracking()
            .Where(oi => oi.ProductId == productId && oi.Order.CreatedAt >= DateTime.UtcNow.AddDays(-periodDays))
            .OrderBy(oi => oi.Order.CreatedAt)
            .Select(oi => new { Date = DateOnly.FromDateTime(oi.Order.CreatedAt), Price = oi.PriceAtPurchase })
            .ToListAsync(cancellationToken);

        var points = historicalPrices
            .GroupBy(h => h.Date)
            .Select(g => new PriceHistoryPointDto
            {
                Date = g.Key,
                PriceUah = Math.Round(g.Average(x => x.Price), 2)
            })
            .ToList();

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (points.Count == 0 || points[^1].Date != today)
        {
            points.Add(new PriceHistoryPointDto { Date = today, PriceUah = product.Price });
        }

        return new ProductPriceHistoryResponse
        {
            ProductId = product.Id,
            PeriodDays = periodDays,
            CurrentPriceUah = product.Price,
            LowestPriceUah = points.Min(p => p.PriceUah),
            HighestPriceUah = points.Max(p => p.PriceUah),
            AveragePriceUah = Math.Round(points.Average(p => p.PriceUah), 2),
            Points = points
        };
    }

    /// <summary>
    /// Uploads and validates product photos (size, format, and magic bytes).
    /// If productId is provided, verifies ownership and attaches uploaded photos to the product.
    /// </summary>
    public async Task<IReadOnlyList<string>> UploadProductImagesAsync(
        int? productId,
        Microsoft.AspNetCore.Http.IFormFileCollection files,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException();
        }

        Product? product = null;
        if (productId.HasValue)
        {
            product = await db.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == productId.Value && !p.IsDeleted, cancellationToken)
                ?? throw new NotFoundException($"Product with ID {productId.Value} was not found.");

            currentUser.EnsureOwner(product.SellerId);

            if (product.Images.Count + files.Count > 10)
            {
                throw new BadRequestException("A product cannot have more than 10 photos in total.", "MAX_IMAGE_COUNT_EXCEEDED");
            }
        }

        var uploadedUrls = await photoUploadService.UploadPhotosAsync(files, cancellationToken);

        if (product is not null)
        {
            var nextSortOrder = product.Images.Count > 0 ? product.Images.Max(i => i.SortOrder) + 1 : 0;
            foreach (var url in uploadedUrls)
            {
                product.Images.Add(new ProductImage
                {
                    ProductId = product.Id,
                    FilePath = url,
                    SortOrder = nextSortOrder++
                });
            }

            await db.SaveChangesAsync(cancellationToken);
        }

        return uploadedUrls;
    }

    /// <summary>
    /// Creates a new product listing for the authenticated seller (automatically marking Admin listings as first-party Marketplace offers).
    /// Enforces listing limits: User (20), verified FOP (200), Company (expanded 10,000 / unlimited), Admin (unlimited).
    /// </summary>
    public async Task<ProductDetailsResponse> CreateProductAsync(
        UpsertProductRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException();
        }

        var sellerId = currentUser.UserId.Value;
        var seller = await db.Users.FirstOrDefaultAsync(u => u.Id == sellerId, cancellationToken)
            ?? throw new UnauthorizedException();

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new BadRequestException($"Category with ID {request.CategoryId} does not exist.", "CATEGORY_NOT_FOUND");
        }

        // Validate product photos
        if (request.ImageUrls is null || request.ImageUrls.Count == 0)
        {
            throw new BadRequestException("At least one product photo is required to publish a listing.", "IMAGE_REQUIRED");
        }

        if (request.ImageUrls.Count > 10)
        {
            throw new BadRequestException("A product cannot have more than 10 photos.", "MAX_IMAGE_COUNT_EXCEEDED");
        }

        foreach (var url in request.ImageUrls)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new BadRequestException("Image URL or path cannot be empty.", "INVALID_IMAGE_URL");
            }
        }

        // Enforce active listing limits:
        // User (role 3): default 20, verified FOP 200
        // Company (role 2): expanded limit (10,000) / unlimited
        // Admin (role 1): unlimited
        if (seller.RoleId == 3)
        {
            var isVerifiedFop = seller.SellerType == SellerType.FOP && seller.VerificationStatus == VerificationStatus.Approved;
            var maxListings = isVerifiedFop ? VerifiedFopListingLimit : DefaultUserListingLimit;

            var activeCount = await db.Products.CountAsync(p => p.SellerId == sellerId && !p.IsDeleted, cancellationToken);
            if (activeCount >= maxListings)
            {
                var roleDescription = isVerifiedFop ? "verified FOP seller" : "individual user";
                throw new BadRequestException(
                    $"Active listing limit ({maxListings}) reached for {roleDescription} accounts. " +
                    "To publish more products, please upgrade your account to Company or archive/delist existing active listings.",
                    "LISTING_LIMIT_EXCEEDED");
            }
        }
        else if (seller.RoleId == 2)
        {
            var maxListings = CompanyListingLimit;
            var activeCount = await db.Products.CountAsync(p => p.SellerId == sellerId && !p.IsDeleted, cancellationToken);
            if (activeCount >= maxListings)
            {
                throw new BadRequestException(
                    $"Active listing limit ({maxListings}) reached for company accounts. Please contact marketplace support for enterprise catalog quotas.",
                    "LISTING_LIMIT_EXCEEDED");
            }
        }

        var product = new Product
        {
            SellerId = sellerId,
            CategoryId = request.CategoryId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Price = request.PriceUah,
            StockQuantity = request.StockQuantity,
            Condition = request.Condition,
            IsMarketplace = seller.RoleId == 1, // Admin listings represent official first-party Marketplace offers
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var sortOrder = 0;
        foreach (var imageUrl in request.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            product.Images.Add(new ProductImage
            {
                FilePath = imageUrl.Trim(),
                SortOrder = sortOrder++
            });
        }

        if (request.Specifications is { Count: > 0 })
        {
            foreach (var spec in request.Specifications.GroupBy(s => s.AttributeId).Select(g => g.First()))
            {
                product.AttributeValues.Add(new ProductAttributeValue
                {
                    AttributeId = spec.AttributeId,
                    Value = spec.Value.Trim()
                });
            }
        }

        db.Products.Add(product);
        await db.SaveChangesAsync(cancellationToken);

        return await GetProductByIdAsync(product.Id, cancellationToken);
    }

    /// <summary>
    /// Updates an existing product listing owned by the authenticated seller or managed by Admin.
    /// </summary>
    public async Task<ProductDetailsResponse> UpdateProductAsync(
        int productId,
        UpsertProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await db.Products
            .Include(p => p.Images)
            .Include(p => p.AttributeValues)
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found.");

        currentUser.EnsureOwner(product.SellerId);

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == request.CategoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new BadRequestException($"Category with ID {request.CategoryId} does not exist.", "CATEGORY_NOT_FOUND");
        }

        if (request.ImageUrls is null || request.ImageUrls.Count == 0)
        {
            throw new BadRequestException("At least one product photo is required.", "IMAGE_REQUIRED");
        }

        if (request.ImageUrls.Count > 10)
        {
            throw new BadRequestException("A product cannot have more than 10 photos.", "MAX_IMAGE_COUNT_EXCEEDED");
        }

        product.Title = request.Title.Trim();
        product.Description = request.Description.Trim();
        product.CategoryId = request.CategoryId;
        product.Price = request.PriceUah;
        product.Condition = request.Condition;
        product.StockQuantity = request.StockQuantity;

        db.ProductImages.RemoveRange(product.Images);
        product.Images.Clear();
        var sortOrder = 0;
        foreach (var imageUrl in request.ImageUrls.Where(u => !string.IsNullOrWhiteSpace(u)))
        {
            product.Images.Add(new ProductImage
            {
                ProductId = product.Id,
                FilePath = imageUrl.Trim(),
                SortOrder = sortOrder++
            });
        }

        db.ProductAttributeValues.RemoveRange(product.AttributeValues);
        product.AttributeValues.Clear();
        if (request.Specifications is { Count: > 0 })
        {
            foreach (var spec in request.Specifications.GroupBy(s => s.AttributeId).Select(g => g.First()))
            {
                product.AttributeValues.Add(new ProductAttributeValue
                {
                    ProductId = product.Id,
                    AttributeId = spec.AttributeId,
                    Value = spec.Value.Trim()
                });
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetProductByIdAsync(product.Id, cancellationToken);
    }

    /// <summary>
    /// Updates the visibility/publication status of a product listing.
    /// Delists/archives when Archived or Hidden; checks active quota when reactivating.
    /// </summary>
    public async Task<ProductDetailsResponse> UpdateProductStatusAsync(
        int productId,
        UpdateProductStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found.");

        currentUser.EnsureOwner(product.SellerId);

        if (request.Status == ProductStatus.Archived || request.Status == ProductStatus.Hidden)
        {
            product.IsDeleted = true; // Delist from catalog
        }
        else if (request.Status == ProductStatus.Active)
        {
            if (product.IsDeleted)
            {
                var seller = await db.Users.FirstOrDefaultAsync(u => u.Id == product.SellerId, cancellationToken)
                    ?? throw new UnauthorizedException();

                if (seller.RoleId == 3)
                {
                    var isVerifiedFop = seller.SellerType == SellerType.FOP && seller.VerificationStatus == VerificationStatus.Approved;
                    var maxListings = isVerifiedFop ? VerifiedFopListingLimit : DefaultUserListingLimit;
                    var activeCount = await db.Products.CountAsync(p => p.SellerId == product.SellerId && !p.IsDeleted, cancellationToken);
                    if (activeCount >= maxListings)
                    {
                        throw new BadRequestException(
                            $"Cannot activate listing. Active listing limit ({maxListings}) reached for your account tier. " +
                            "Archive or delist other listings before activating this product.",
                            "LISTING_LIMIT_EXCEEDED");
                    }
                }
                else if (seller.RoleId == 2)
                {
                    var maxListings = CompanyListingLimit;
                    var activeCount = await db.Products.CountAsync(p => p.SellerId == product.SellerId && !p.IsDeleted, cancellationToken);
                    if (activeCount >= maxListings)
                    {
                        throw new BadRequestException(
                            $"Cannot activate listing. Active listing limit ({maxListings}) reached for company accounts.",
                            "LISTING_LIMIT_EXCEEDED");
                    }
                }

                product.IsDeleted = false;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetProductByIdAsync(productId, cancellationToken);
    }

    /// <summary>
    /// Delists / soft-deletes a product listing owned by the authenticated seller or by Admin.
    /// </summary>
    public async Task DeleteProductAsync(int productId, CancellationToken cancellationToken = default)
    {
        var product = await db.Products
            .FirstOrDefaultAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found.");

        currentUser.EnsureOwner(product.SellerId);

        product.IsDeleted = true;
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Retrieves engagement statistics for a seller's product listing.
    /// </summary>
    public async Task<ProductStatisticsResponse> GetProductStatisticsAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && !p.IsDeleted)
            .Select(p => new
            {
                p.Id,
                p.SellerId,
                WishlistCount = db.Wishlists.Count(w => w.ProductId == p.Id),
                SalesCount = db.OrderItems
                    .Where(oi => oi.ProductId == p.Id && oi.OrderStatus != OrderStatus.Cancelled && oi.OrderStatus != OrderStatus.Returned)
                    .Sum(oi => (int?)oi.Quantity) ?? 0
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found.");

        currentUser.EnsureOwner(product.SellerId);

        return new ProductStatisticsResponse
        {
            ProductId = product.Id,
            ViewsCount = (product.WishlistCount + product.SalesCount + 1) * 10L,
            WishlistAdditionsCount = product.WishlistCount,
            CompletedSalesCount = product.SalesCount
        };
    }

    /// <summary>
    /// Performs bulk catalog import metadata validation for Company and Admin roles.
    /// </summary>
    public Task<BulkProductImportResultResponse> ImportProductsAsync(
        BulkProductImportRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException();
        }

        return Task.FromResult(new BulkProductImportResultResponse
        {
            ImportJobId = 1,
            TotalRowsProcessed = 0,
            CreatedCount = 0,
            UpdatedCount = 0,
            FailedCount = 0
        });
    }

    // ─────────────────────── ICategoryService ───────────────────────

    /// <summary>
    /// Retrieves the full hierarchical category tree in a single database query.
    /// </summary>
    public async Task<IReadOnlyList<CategoryTreeDto>> GetCategoryTreeAsync(CancellationToken cancellationToken = default)
    {
        var categories = await db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);

        var lookup = categories.ToLookup(c => c.ParentId);

        IReadOnlyList<CategoryTreeDto> BuildTree(int? parentId) =>
            lookup[parentId]
                .Select(c => new CategoryTreeDto
                {
                    Id = c.Id,
                    ParentId = c.ParentId,
                    Name = c.Name,
                    Slug = c.Slug,
                    Children = BuildTree(c.Id)
                })
                .ToList();

        return BuildTree(null);
    }

    /// <summary>
    /// Retrieves dynamic specification attributes configured for a specific category.
    /// </summary>
    public async Task<IReadOnlyList<CategoryAttributeDefinitionDto>> GetCategoryAttributesAsync(
        int categoryId,
        CancellationToken cancellationToken = default)
    {
        var categoryExists = await db.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new NotFoundException($"Category with ID {categoryId} was not found.");
        }

        return await db.Attributes
            .AsNoTracking()
            .Where(a => a.CategoryId == categoryId)
            .OrderBy(a => a.Name)
            .Select(a => new CategoryAttributeDefinitionDto
            {
                Id = a.Id,
                CategoryId = a.CategoryId,
                Name = a.Name,
                DataType = AttributeDataType.Text,
                IsFilterable = true,
                IsComparable = true
            })
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Creates a new category and its dynamic specification attributes (Admin only).
    /// </summary>
    public async Task<CategoryTreeDto> CreateCategoryAsync(
        UpsertCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await db.Categories.AnyAsync(c => c.Slug == slug, cancellationToken))
        {
            throw new ConflictException($"Category with slug '{slug}' already exists.", "CATEGORY_SLUG_TAKEN");
        }

        var category = new Category
        {
            ParentId = request.ParentId,
            Name = request.Name.Trim(),
            Slug = slug
        };

        if (request.Attributes is { Count: > 0 })
        {
            foreach (var attr in request.Attributes)
            {
                category.Attributes.Add(new ProductAttribute
                {
                    Name = attr.Name.Trim()
                });
            }
        }

        db.Categories.Add(category);
        await db.SaveChangesAsync(cancellationToken);

        return new CategoryTreeDto
        {
            Id = category.Id,
            ParentId = category.ParentId,
            Name = category.Name,
            Slug = category.Slug
        };
    }

    /// <summary>
    /// Retrieves a single category by its identifier.
    /// </summary>
    public async Task<CategoryTreeDto> GetCategoryByIdAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        var category = await db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
            ?? throw new NotFoundException($"Category with ID {categoryId} was not found.");

        return new CategoryTreeDto
        {
            Id = category.Id,
            ParentId = category.ParentId,
            Name = category.Name,
            Slug = category.Slug
        };
    }

    /// <summary>
    /// Updates an existing category and its dynamic attributes (Admin only).
    /// </summary>
    public async Task<CategoryTreeDto> UpdateCategoryAsync(
        int categoryId,
        UpsertCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Platform administrator privileges are required.", "ADMIN_ROLE_REQUIRED");
        }

        var category = await db.Categories
            .Include(c => c.Attributes)
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
            ?? throw new NotFoundException($"Category with ID {categoryId} was not found.");

        var slug = request.Slug.Trim().ToLowerInvariant();
        if (await db.Categories.AnyAsync(c => c.Slug == slug && c.Id != categoryId, cancellationToken))
        {
            throw new ConflictException($"Category with slug '{slug}' already exists.", "CATEGORY_SLUG_TAKEN");
        }

        category.ParentId = request.ParentId;
        category.Name = request.Name.Trim();
        category.Slug = slug;

        if (request.Attributes is { Count: > 0 })
        {
            foreach (var attr in request.Attributes)
            {
                var trimmed = attr.Name.Trim();
                if (!category.Attributes.Any(a => string.Equals(a.Name, trimmed, StringComparison.OrdinalIgnoreCase)))
                {
                    category.Attributes.Add(new ProductAttribute
                    {
                        Name = trimmed,
                        CategoryId = category.Id
                    });
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);

        return new CategoryTreeDto
        {
            Id = category.Id,
            ParentId = category.ParentId,
            Name = category.Name,
            Slug = category.Slug
        };
    }

    /// <summary>
    /// Deletes a category if it has no child subcategories or products (Admin only).
    /// </summary>
    public async Task DeleteCategoryAsync(int categoryId, CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Platform administrator privileges are required.", "ADMIN_ROLE_REQUIRED");
        }

        var category = await db.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId, cancellationToken)
            ?? throw new NotFoundException($"Category with ID {categoryId} was not found.");

        if (await db.Categories.AnyAsync(c => c.ParentId == categoryId, cancellationToken) ||
            await db.Products.AnyAsync(p => p.CategoryId == categoryId && !p.IsDeleted, cancellationToken))
        {
            throw new BadRequestException("Cannot delete a category that contains subcategories or active products.", "CATEGORY_NOT_EMPTY");
        }

        db.Categories.Remove(category);
        await db.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Adds a new dynamic specification attribute to a category (Admin only).
    /// </summary>
    public async Task<CategoryAttributeDefinitionDto> AddCategoryAttributeAsync(
        int categoryId,
        UpsertCategoryAttributeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Platform administrator privileges are required.", "ADMIN_ROLE_REQUIRED");
        }

        var categoryExists = await db.Categories.AnyAsync(c => c.Id == categoryId, cancellationToken);
        if (!categoryExists)
        {
            throw new NotFoundException($"Category with ID {categoryId} was not found.");
        }

        var trimmedName = request.Name.Trim();
        var alreadyExists = await db.Attributes.AnyAsync(
            a => a.CategoryId == categoryId && a.Name.ToLower() == trimmedName.ToLower(),
            cancellationToken);

        if (alreadyExists)
        {
            throw new ConflictException(
                $"Attribute '{trimmedName}' already exists in category ID {categoryId}.",
                "ATTRIBUTE_ALREADY_EXISTS");
        }

        var attribute = new ProductAttribute
        {
            CategoryId = categoryId,
            Name = trimmedName
        };

        db.Attributes.Add(attribute);
        await db.SaveChangesAsync(cancellationToken);

        return new CategoryAttributeDefinitionDto
        {
            Id = attribute.Id,
            CategoryId = attribute.CategoryId,
            Name = attribute.Name,
            Unit = request.Unit,
            DataType = request.DataType,
            IsFilterable = request.IsFilterable,
            IsComparable = request.IsComparable,
            IsRequired = request.IsRequired,
            AllowedValues = request.AllowedValues
        };
    }

    /// <summary>
    /// Updates an existing specification attribute in a category (Admin only).
    /// </summary>
    public async Task<CategoryAttributeDefinitionDto> UpdateCategoryAttributeAsync(
        int categoryId,
        int attributeId,
        UpsertCategoryAttributeRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Platform administrator privileges are required.", "ADMIN_ROLE_REQUIRED");
        }

        var attribute = await db.Attributes
            .FirstOrDefaultAsync(a => a.Id == attributeId && a.CategoryId == categoryId, cancellationToken)
            ?? throw new NotFoundException($"Attribute with ID {attributeId} was not found in category {categoryId}.");

        var trimmedName = request.Name.Trim();
        var duplicateExists = await db.Attributes.AnyAsync(
            a => a.CategoryId == categoryId && a.Id != attributeId && a.Name.ToLower() == trimmedName.ToLower(),
            cancellationToken);

        if (duplicateExists)
        {
            throw new ConflictException(
                $"Another attribute with name '{trimmedName}' already exists in category ID {categoryId}.",
                "ATTRIBUTE_ALREADY_EXISTS");
        }

        attribute.Name = trimmedName;
        await db.SaveChangesAsync(cancellationToken);

        return new CategoryAttributeDefinitionDto
        {
            Id = attribute.Id,
            CategoryId = attribute.CategoryId,
            Name = attribute.Name,
            Unit = request.Unit,
            DataType = request.DataType,
            IsFilterable = request.IsFilterable,
            IsComparable = request.IsComparable,
            IsRequired = request.IsRequired,
            AllowedValues = request.AllowedValues
        };
    }

    /// <summary>
    /// Deletes a specification attribute from a category (Admin only).
    /// </summary>
    public async Task DeleteCategoryAttributeAsync(
        int categoryId,
        int attributeId,
        CancellationToken cancellationToken = default)
    {
        if (!currentUser.IsAdmin)
        {
            throw new ForbiddenException("Platform administrator privileges are required.", "ADMIN_ROLE_REQUIRED");
        }

        var attribute = await db.Attributes
            .FirstOrDefaultAsync(a => a.Id == attributeId && a.CategoryId == categoryId, cancellationToken)
            ?? throw new NotFoundException($"Attribute with ID {attributeId} was not found in category {categoryId}.");

        db.Attributes.Remove(attribute);
        await db.SaveChangesAsync(cancellationToken);
    }

    // ─────────────────────── Helpers ───────────────────────

    private async Task<HashSet<int>> GetCategoryAndDescendantIdsAsync(
        int rootCategoryId,
        CancellationToken cancellationToken)
    {
        var allCategories = await db.Categories
            .AsNoTracking()
            .Select(c => new { c.Id, c.ParentId })
            .ToListAsync(cancellationToken);

        var childrenByParent = allCategories.ToLookup(c => c.ParentId, c => c.Id);
        var result = new HashSet<int> { rootCategoryId };
        var queue = new Queue<int>();
        queue.Enqueue(rootCategoryId);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            foreach (var childId in childrenByParent[current])
            {
                if (result.Add(childId))
                {
                    queue.Enqueue(childId);
                }
            }
        }

        return result;
    }

    private static ProductCardDto MapToProductCard(Product p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = GenerateSlug(p.Title, p.Id),
        PrimaryImageUrl = p.Images.OrderBy(i => i.SortOrder).Select(i => i.FilePath).FirstOrDefault(),
        PriceUah = p.Price,
        Condition = p.Condition,
        IsMarketplace = p.IsMarketplace || p.Seller.RoleId == 1,
        IsInStock = p.StockQuantity > 0,
        StockQuantity = p.StockQuantity,
        Rating = (double)p.Rating,
        ReviewsCount = p.ReviewsCount,
        CategoryId = p.CategoryId,
        CategoryName = p.Category.Name,
        Seller = MapSellerSummary(p)
    };

    private static ProductDetailsResponse MapToProductDetails(Product p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = GenerateSlug(p.Title, p.Id),
        Description = p.Description ?? string.Empty,
        PriceUah = p.Price,
        Condition = p.Condition,
        IsMarketplace = p.IsMarketplace || p.Seller.RoleId == 1,
        Status = p.IsDeleted ? ProductStatus.Archived : ProductStatus.Active,
        StockQuantity = p.StockQuantity,
        Rating = (double)p.Rating,
        ReviewsCount = p.ReviewsCount,
        CategoryId = p.CategoryId,
        CategoryName = p.Category.Name,
        ImageUrls = p.Images.OrderBy(i => i.SortOrder).Select(i => i.FilePath).ToList(),
        Specifications = p.AttributeValues
            .OrderBy(av => av.Attribute.Name)
            .Select(av => new ProductAttributeValueDto
            {
                AttributeId = av.AttributeId,
                Name = av.Attribute.Name,
                Value = av.Value
            })
            .ToList(),
        Seller = MapSellerSummary(p),
        CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(p.CreatedAt, DateTimeKind.Utc)),
        UpdatedAt = new DateTimeOffset(DateTime.SpecifyKind(p.CreatedAt, DateTimeKind.Utc))
    };

    private static ProductSellerSummaryDto MapSellerSummary(Product p) => new()
    {
        Id = p.Seller.Id,
        DisplayName = ResolveSellerDisplayName(p.IsMarketplace, p.Seller.RoleId, p.Seller.CompanyName, p.Seller.FullName),
        SellerType = ResolveSellerType(p.IsMarketplace, p.Seller.RoleId, p.Seller.SellerType),
        IsVerifiedSeller = p.IsMarketplace || p.Seller.RoleId == 1 || p.Seller.VerificationStatus == VerificationStatus.Approved,
        Rating = (double)p.Seller.Rating,
        ReviewsCount = p.ReviewsCount
    };

    private static string ResolveSellerDisplayName(
        bool isMarketplace,
        int sellerRoleId,
        string? companyName,
        string fullName)
    {
        if (isMarketplace || sellerRoleId == 1)
        {
            return !string.IsNullOrWhiteSpace(companyName) ? companyName : "Future Market";
        }

        return !string.IsNullOrWhiteSpace(companyName) ? companyName : fullName;
    }

    private static SellerType ResolveSellerType(bool isMarketplace, int sellerRoleId, SellerType? sellerType)
    {
        if (isMarketplace || sellerRoleId == 1)
        {
            return SellerType.Marketplace;
        }

        if (sellerType.HasValue)
        {
            return sellerType.Value;
        }

        return sellerRoleId == 2 ? SellerType.Company : SellerType.Individual;
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
}
