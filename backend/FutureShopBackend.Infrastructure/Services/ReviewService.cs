using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Interaction;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using FutureShopBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FutureShopBackend.Infrastructure.Services;

/// <summary>
/// Implements product and seller reviews (1-5 star ratings), server-side verified purchase enforcement,
/// and automatic recalculation of product and seller average ratings.
/// </summary>
public sealed class ReviewService(
    AppDbContext db,
    ICurrentUserService currentUser) : IReviewService
{
    /// <summary>
    /// Retrieves paginated reviews for a specific product in a single projected query.
    /// </summary>
    public async Task<PagedResponse<ReviewDto>> GetProductReviewsAsync(
        int productId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var productExists = await db.Products
            .AsNoTracking()
            .AnyAsync(p => p.Id == productId && !p.IsDeleted, cancellationToken);

        if (!productExists)
        {
            throw new NotFoundException($"Product with ID {productId} was not found.");
        }

        var baseQuery = db.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id);

        return await ProjectPagedReviewsAsync(baseQuery, query, cancellationToken);
    }

    /// <summary>
    /// Retrieves paginated reviews received by a specific seller (including reviews on the seller's products).
    /// </summary>
    public async Task<PagedResponse<ReviewDto>> GetSellerReviewsAsync(
        int sellerId,
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var sellerExists = await db.Users
            .AsNoTracking()
            .AnyAsync(u => u.Id == sellerId, cancellationToken);

        if (!sellerExists)
        {
            throw new NotFoundException($"Seller with ID {sellerId} was not found.");
        }

        var baseQuery = db.Reviews
            .AsNoTracking()
            .Where(r => r.SellerId == sellerId || (r.Product != null && r.Product.SellerId == sellerId))
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id);

        return await ProjectPagedReviewsAsync(baseQuery, query, cancellationToken);
    }

    /// <summary>
    /// Retrieves paginated reviews written by the currently authenticated user.
    /// </summary>
    public async Task<PagedResponse<ReviewDto>> GetMyWrittenReviewsAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var baseQuery = db.Reviews
            .AsNoTracking()
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ThenByDescending(r => r.Id);

        return await ProjectPagedReviewsAsync(baseQuery, query, cancellationToken);
    }

    /// <summary>
    /// Creates a new review (1-5 stars) for a product and/or seller after verifying that the user purchased the product,
    /// and automatically recalculates the average rating of both the product and the seller.
    /// </summary>
    public async Task<ReviewDto> CreateReviewAsync(
        CreateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        if (request.Rating < 1 || request.Rating > 5)
        {
            throw new BadRequestException("Rating must be between 1 and 5 stars.", "INVALID_RATING");
        }

        if (!request.ProductId.HasValue && !request.SellerId.HasValue)
        {
            throw new BadRequestException(
                "Either ProductId or SellerId must be provided to submit a review.",
                "REVIEW_TARGET_REQUIRED");
        }

        var author = await db.Users
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new { u.Id, u.FullName, u.AvatarUrl })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new UnauthorizedException();

        Product? targetProduct = null;
        int targetSellerId;

        if (request.ProductId.HasValue)
        {
            targetProduct = await db.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId.Value && !p.IsDeleted, cancellationToken)
                ?? throw new NotFoundException($"Product with ID {request.ProductId.Value} was not found.");

            targetSellerId = targetProduct.SellerId;

            if (targetSellerId == userId)
            {
                throw new ForbiddenException("You cannot leave a review on your own product.", "SELF_REVIEW_FORBIDDEN");
            }

            // Server-side verified purchase check: block reviews if the user has not purchased this product.
            var purchaseQuery = db.OrderItems
                .AsNoTracking()
                .Where(oi =>
                    oi.Order.UserId == userId &&
                    oi.ProductId == targetProduct.Id &&
                    oi.Order.OrderStatus != OrderStatus.Cancelled &&
                    oi.Order.OrderStatus != OrderStatus.Returned &&
                    oi.OrderStatus != OrderStatus.Cancelled &&
                    oi.OrderStatus != OrderStatus.Returned);

            if (request.OrderId.HasValue)
            {
                purchaseQuery = purchaseQuery.Where(oi => oi.OrderId == request.OrderId.Value);
            }

            var hasPurchasedProduct = await purchaseQuery.AnyAsync(cancellationToken);
            if (!hasPurchasedProduct)
            {
                throw new ForbiddenException(
                    "Only users who have purchased this product can leave a review.",
                    "PURCHASE_REQUIRED");
            }
        }
        else
        {
            targetSellerId = request.SellerId!.Value;
            var sellerExists = await db.Users
                .AsNoTracking()
                .AnyAsync(u => u.Id == targetSellerId, cancellationToken);

            if (!sellerExists)
            {
                throw new NotFoundException($"Seller with ID {targetSellerId} was not found.");
            }

            if (targetSellerId == userId)
            {
                throw new ForbiddenException("You cannot leave a review for yourself.", "SELF_REVIEW_FORBIDDEN");
            }

            // Server-side verified purchase check: verify the buyer purchased at least one product from this seller.
            var purchaseQuery = db.OrderItems
                .AsNoTracking()
                .Where(oi =>
                    oi.Order.UserId == userId &&
                    oi.SellerId == targetSellerId &&
                    oi.Order.OrderStatus != OrderStatus.Cancelled &&
                    oi.Order.OrderStatus != OrderStatus.Returned &&
                    oi.OrderStatus != OrderStatus.Cancelled &&
                    oi.OrderStatus != OrderStatus.Returned);

            if (request.OrderId.HasValue)
            {
                purchaseQuery = purchaseQuery.Where(oi => oi.OrderId == request.OrderId.Value);
            }

            var hasPurchasedFromSeller = await purchaseQuery.AnyAsync(cancellationToken);
            if (!hasPurchasedFromSeller)
            {
                throw new ForbiddenException(
                    "Only users who have purchased a product from this seller can leave a review.",
                    "PURCHASE_REQUIRED");
            }
        }

        var review = new Review
        {
            UserId = userId,
            ProductId = targetProduct?.Id,
            SellerId = targetSellerId,
            Rating = request.Rating,
            Comment = string.IsNullOrWhiteSpace(request.Comment) ? null : request.Comment.Trim(),
            Pros = string.IsNullOrWhiteSpace(request.Pros) ? null : request.Pros.Trim(),
            Cons = string.IsNullOrWhiteSpace(request.Cons) ? null : request.Cons.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        db.Reviews.Add(review);
        await db.SaveChangesAsync(cancellationToken);

        // Automatically recalculate average rating for both the product and the seller.
        await RecalculateRatingsAsync(targetProduct?.Id, targetSellerId, cancellationToken);

        return new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductTitle = targetProduct?.Title,
            SellerId = review.SellerId,
            AuthorId = author.Id,
            AuthorName = author.FullName,
            AuthorAvatarUrl = author.AvatarUrl,
            Rating = review.Rating,
            Comment = review.Comment ?? string.Empty,
            Pros = review.Pros,
            Cons = review.Cons,
            IsVerifiedPurchase = true,
            SellerReply = review.SellerReply,
            SellerRepliedAt = null,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(review.CreatedAt, DateTimeKind.Utc))
        };
    }

    /// <summary>
    /// Posts a seller reply to an existing review on the seller's product or profile.
    /// </summary>
    public async Task<ReviewDto> ReplyToReviewAsync(
        int reviewId,
        ReplyToReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var review = await db.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken)
            ?? throw new NotFoundException($"Review with ID {reviewId} was not found.");

        var resolvedSellerId = review.SellerId ?? review.Product?.SellerId
            ?? throw new ForbiddenException("Unable to determine the seller for this review.");

        currentUser.EnsureOwner(resolvedSellerId);

        review.SellerReply = request.ReplyText.Trim();
        review.SellerRepliedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return new ReviewDto
        {
            Id = review.Id,
            ProductId = review.ProductId,
            ProductTitle = review.Product?.Title,
            SellerId = resolvedSellerId,
            AuthorId = review.UserId,
            AuthorName = review.User.FullName,
            AuthorAvatarUrl = review.User.AvatarUrl,
            Rating = review.Rating,
            Comment = review.Comment ?? string.Empty,
            Pros = review.Pros,
            Cons = review.Cons,
            IsVerifiedPurchase = true,
            SellerReply = review.SellerReply,
            SellerRepliedAt = new DateTimeOffset(DateTime.SpecifyKind(review.SellerRepliedAt.Value, DateTimeKind.Utc)),
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(review.CreatedAt, DateTimeKind.Utc))
        };
    }

    /// <summary>
    /// Deletes a review (by author or Admin) and automatically recalculates the product and seller average ratings.
    /// </summary>
    public async Task DeleteReviewAsync(int reviewId, CancellationToken cancellationToken = default)
    {
        var review = await db.Reviews
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken)
            ?? throw new NotFoundException($"Review with ID {reviewId} was not found.");

        currentUser.EnsureOwner(review.UserId);

        var productId = review.ProductId;
        var sellerId = review.SellerId ?? review.Product?.SellerId;

        db.Reviews.Remove(review);
        await db.SaveChangesAsync(cancellationToken);

        await RecalculateRatingsAsync(productId, sellerId, cancellationToken);
    }

    /// <summary>
    /// Automatically recalculates and persists the average star rating for the affected product and seller.
    /// </summary>
    private async Task RecalculateRatingsAsync(
        int? productId,
        int? sellerId,
        CancellationToken cancellationToken)
    {
        if (productId.HasValue)
        {
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == productId.Value, cancellationToken);
            if (product is not null)
            {
                var productStats = await db.Reviews
                    .AsNoTracking()
                    .Where(r => r.ProductId == product.Id)
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Count = g.Count(),
                        Average = g.Average(r => (double)r.Rating)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                product.ReviewsCount = productStats?.Count ?? 0;
                product.Rating = productStats is not null
                    ? Math.Round((decimal)productStats.Average, 2)
                    : 0m;
            }
        }

        if (sellerId.HasValue)
        {
            var seller = await db.Users.FirstOrDefaultAsync(u => u.Id == sellerId.Value, cancellationToken);
            if (seller is not null)
            {
                var sellerStats = await db.Reviews
                    .AsNoTracking()
                    .Where(r => r.SellerId == seller.Id || (r.Product != null && r.Product.SellerId == seller.Id))
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Count = g.Count(),
                        Average = g.Average(r => (double)r.Rating)
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                seller.Rating = sellerStats is not null
                    ? Math.Round((decimal)sellerStats.Average, 2)
                    : 0m;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<PagedResponse<ReviewDto>> ProjectPagedReviewsAsync(
        IQueryable<Review> baseQuery,
        PaginationQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var rawItems = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.ProductId,
                ProductTitle = r.Product != null ? r.Product.Title : null,
                SellerId = r.SellerId ?? (r.Product != null ? (int?)r.Product.SellerId : null),
                AuthorId = r.UserId,
                AuthorName = r.User.FullName,
                AuthorAvatarUrl = r.User.AvatarUrl,
                r.Rating,
                r.Comment,
                r.Pros,
                r.Cons,
                r.SellerReply,
                r.SellerRepliedAt,
                r.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var items = rawItems.Select(r => new ReviewDto
        {
            Id = r.Id,
            ProductId = r.ProductId,
            ProductTitle = r.ProductTitle,
            SellerId = r.SellerId,
            AuthorId = r.AuthorId,
            AuthorName = r.AuthorName,
            AuthorAvatarUrl = r.AuthorAvatarUrl,
            Rating = r.Rating,
            Comment = r.Comment ?? string.Empty,
            Pros = r.Pros,
            Cons = r.Cons,
            IsVerifiedPurchase = true,
            SellerReply = r.SellerReply,
            SellerRepliedAt = r.SellerRepliedAt.HasValue
                ? new DateTimeOffset(DateTime.SpecifyKind(r.SellerRepliedAt.Value, DateTimeKind.Utc))
                : null,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(r.CreatedAt, DateTimeKind.Utc))
        }).ToList();

        return new PagedResponse<ReviewDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private int RequireAuthenticatedUserId()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException();
        }

        return currentUser.UserId.Value;
    }
}
