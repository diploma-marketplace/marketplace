using FutureShopBackend.Application.DTO.Catalog;
using FutureShopBackend.Application.DTO.Commerce;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using FutureShopBackend.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FutureShopBackend.Infrastructure.Services;

/// <summary>
/// Implements persistent server-side shopping cart (with automatic seller grouping and total cost calculation),
/// buyer wishlist with price-drop tracking, and order checkout from the cart.
/// </summary>
public sealed class CommerceService(
    AppDbContext db,
    ICurrentUserService currentUser) : ICartService, IWishlistService, IOrderService
{
    // ─────────────────────── ICartService ───────────────────────

    /// <summary>
    /// Retrieves the authenticated buyer's persistent shopping cart grouped by sellers with calculated subtotals and grand total.
    /// </summary>
    public async Task<CartResponse> GetMyCartAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        // Fetch all active cart items and their seller/product details in a single projected query (no N+1).
        var rawItems = await db.CartItems
            .AsNoTracking()
            .Where(ci => ci.Cart.UserId == userId && !ci.Product.IsDeleted)
            .OrderBy(ci => ci.Product.SellerId)
            .ThenBy(ci => ci.Id)
            .Select(ci => new
            {
                ci.ProductId,
                ci.Quantity,
                ProductTitle = ci.Product.Title,
                UnitPrice = ci.Product.Price,
                AvailableStock = ci.Product.StockQuantity,
                IsMarketplace = ci.Product.IsMarketplace,
                ImageUrl = ci.Product.Images
                    .OrderBy(img => img.SortOrder)
                    .Select(img => img.FilePath)
                    .FirstOrDefault(),
                SellerId = ci.Product.Seller.Id,
                SellerFullName = ci.Product.Seller.FullName,
                SellerCompanyName = ci.Product.Seller.CompanyName,
                SellerRoleId = ci.Product.Seller.RoleId,
                SellerAccountType = ci.Product.Seller.SellerType,
                SellerVerificationStatus = ci.Product.Seller.VerificationStatus
            })
            .ToListAsync(cancellationToken);

        // Group items by seller and compute per-seller subtotals and overall cart totals.
        var sellerGroups = rawItems
            .GroupBy(i => i.SellerId)
            .Select(group =>
            {
                var first = group.First();
                var items = group.Select(i => new CartItemDto
                {
                    ProductId = i.ProductId,
                    ProductTitle = i.ProductTitle,
                    ProductSlug = GenerateSlug(i.ProductTitle, i.ProductId),
                    ImageUrl = i.ImageUrl,
                    UnitPriceUah = i.UnitPrice,
                    Quantity = i.Quantity,
                    AvailableStock = i.AvailableStock
                }).ToList();

                return new CartSellerGroupDto
                {
                    SellerId = first.SellerId,
                    SellerDisplayName = ResolveSellerDisplayName(
                        first.IsMarketplace,
                        first.SellerRoleId,
                        first.SellerCompanyName,
                        first.SellerFullName),
                    SellerType = ResolveSellerType(
                        first.IsMarketplace,
                        first.SellerRoleId,
                        first.SellerAccountType),
                    IsVerifiedSeller = first.IsMarketplace ||
                        first.SellerRoleId == 1 ||
                        first.SellerVerificationStatus == VerificationStatus.Approved,
                    Items = items,
                    GroupSubtotalUah = items.Sum(x => x.LineTotalUah)
                };
            })
            .ToList();

        return new CartResponse
        {
            SellerGroups = sellerGroups,
            TotalItemsCount = sellerGroups.Sum(g => g.Items.Sum(i => i.Quantity)),
            TotalAmountUah = sellerGroups.Sum(g => g.GroupSubtotalUah)
        };
    }

    /// <summary>
    /// Adds a product to the authenticated buyer's persistent shopping cart or increments its quantity.
    /// </summary>
    public async Task<CartResponse> AddItemAsync(
        AddCartItemRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        if (request.Quantity <= 0)
        {
            throw new BadRequestException("Quantity must be at least 1.", "INVALID_QUANTITY");
        }

        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == request.ProductId && !p.IsDeleted)
            .Select(p => new { p.Id, p.SellerId, p.StockQuantity })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Product with ID {request.ProductId} was not found.");

        if (product.SellerId == userId)
        {
            throw new BadRequestException(
                "You cannot add your own product listing to the shopping cart.",
                "SELF_PURCHASE_FORBIDDEN");
        }

        if (product.StockQuantity <= 0)
        {
            throw new BadRequestException("This product is currently out of stock.", "OUT_OF_STOCK");
        }

        var cart = await db.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (cart is null)
        {
            cart = new Cart { UserId = userId };
            db.Carts.Add(cart);
        }

        var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == product.Id);
        var targetQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;

        if (targetQuantity > product.StockQuantity)
        {
            throw new BadRequestException(
                $"Requested quantity ({targetQuantity}) exceeds available stock ({product.StockQuantity}).",
                "INSUFFICIENT_STOCK");
        }

        if (existingItem is not null)
        {
            existingItem.Quantity = targetQuantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                Quantity = request.Quantity
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetMyCartAsync(cancellationToken);
    }

    /// <summary>
    /// Updates the quantity of a specific product inside the authenticated buyer's shopping cart.
    /// </summary>
    public async Task<CartResponse> UpdateItemQuantityAsync(
        int productId,
        UpdateCartItemQuantityRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        if (request.Quantity <= 0)
        {
            throw new BadRequestException("Quantity must be at least 1.", "INVALID_QUANTITY");
        }

        var cartItem = await db.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.Cart.UserId == userId && ci.ProductId == productId, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found in your cart.");

        if (cartItem.Product.IsDeleted || cartItem.Product.StockQuantity < request.Quantity)
        {
            throw new BadRequestException(
                $"Requested quantity ({request.Quantity}) exceeds available stock ({cartItem.Product.StockQuantity}).",
                "INSUFFICIENT_STOCK");
        }

        cartItem.Quantity = request.Quantity;
        await db.SaveChangesAsync(cancellationToken);

        return await GetMyCartAsync(cancellationToken);
    }

    /// <summary>
    /// Removes a specific product from the authenticated buyer's shopping cart.
    /// </summary>
    public async Task<CartResponse> RemoveItemAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var cartItem = await db.CartItems
            .FirstOrDefaultAsync(ci => ci.Cart.UserId == userId && ci.ProductId == productId, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found in your cart.");

        db.CartItems.Remove(cartItem);
        await db.SaveChangesAsync(cancellationToken);

        return await GetMyCartAsync(cancellationToken);
    }

    /// <summary>
    /// Clears all items from the authenticated buyer's shopping cart.
    /// </summary>
    public async Task ClearCartAsync(CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var items = await db.CartItems
            .Where(ci => ci.Cart.UserId == userId)
            .ToListAsync(cancellationToken);

        if (items.Count > 0)
        {
            db.CartItems.RemoveRange(items);
            await db.SaveChangesAsync(cancellationToken);
        }
    }

    // ─────────────────────── IWishlistService ───────────────────────

    /// <summary>
    /// Retrieves the authenticated buyer's saved wishlist items with price-drop tracking.
    /// </summary>
    public async Task<PagedResponse<WishlistItemDto>> GetMyWishlistAsync(
        PaginationQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var baseQuery = db.Wishlists
            .AsNoTracking()
            .Where(w => w.UserId == userId && !w.Product.IsDeleted)
            .OrderByDescending(w => w.CreatedAt)
            .ThenByDescending(w => w.Id);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var rawItems = await baseQuery
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(w => new
            {
                w.ProductId,
                w.PriceAtAddition,
                w.CreatedAt,
                ProductTitle = w.Product.Title,
                PrimaryImageUrl = w.Product.Images
                    .OrderBy(i => i.SortOrder)
                    .Select(i => i.FilePath)
                    .FirstOrDefault(),
                CurrentPrice = w.Product.Price,
                Condition = w.Product.Condition,
                IsMarketplace = w.Product.IsMarketplace,
                StockQuantity = w.Product.StockQuantity,
                ProductRating = w.Product.Rating,
                ReviewsCount = w.Product.ReviewsCount,
                CategoryId = w.Product.CategoryId,
                CategoryName = w.Product.Category.Name,
                SellerId = w.Product.Seller.Id,
                SellerFullName = w.Product.Seller.FullName,
                SellerCompanyName = w.Product.Seller.CompanyName,
                SellerRoleId = w.Product.Seller.RoleId,
                SellerAccountType = w.Product.Seller.SellerType,
                SellerVerificationStatus = w.Product.Seller.VerificationStatus,
                SellerRating = w.Product.Seller.Rating
            })
            .ToListAsync(cancellationToken);

        var items = rawItems.Select(w => new WishlistItemDto
        {
            ProductId = w.ProductId,
            PriceAtAdditionUah = w.PriceAtAddition > 0 ? w.PriceAtAddition : w.CurrentPrice,
            CurrentPriceUah = w.CurrentPrice,
            AddedAt = new DateTimeOffset(DateTime.SpecifyKind(w.CreatedAt, DateTimeKind.Utc)),
            Product = new ProductCardDto
            {
                Id = w.ProductId,
                Title = w.ProductTitle,
                Slug = GenerateSlug(w.ProductTitle, w.ProductId),
                PrimaryImageUrl = w.PrimaryImageUrl,
                PriceUah = w.CurrentPrice,
                Condition = w.Condition,
                IsMarketplace = w.IsMarketplace || w.SellerRoleId == 1,
                IsInStock = w.StockQuantity > 0,
                StockQuantity = w.StockQuantity,
                Rating = (double)w.ProductRating,
                ReviewsCount = w.ReviewsCount,
                CategoryId = w.CategoryId,
                CategoryName = w.CategoryName,
                Seller = new ProductSellerSummaryDto
                {
                    Id = w.SellerId,
                    DisplayName = ResolveSellerDisplayName(w.IsMarketplace, w.SellerRoleId, w.SellerCompanyName, w.SellerFullName),
                    SellerType = ResolveSellerType(w.IsMarketplace, w.SellerRoleId, w.SellerAccountType),
                    IsVerifiedSeller = w.IsMarketplace || w.SellerRoleId == 1 || w.SellerVerificationStatus == VerificationStatus.Approved,
                    Rating = (double)w.SellerRating,
                    ReviewsCount = w.ReviewsCount
                }
            }
        }).ToList();

        return new PagedResponse<WishlistItemDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    /// <summary>
    /// Saves a product to the authenticated buyer's wishlist and records its price at addition.
    /// </summary>
    public async Task<WishlistItemDto> AddToWishlistAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var product = await db.Products
            .AsNoTracking()
            .Where(p => p.Id == productId && !p.IsDeleted)
            .Include(p => p.Category)
            .Include(p => p.Seller)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found.");

        var existing = await db.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken);

        if (existing is null)
        {
            existing = new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                PriceAtAddition = product.Price,
                CreatedAt = DateTime.UtcNow
            };
            db.Wishlists.Add(existing);
            await db.SaveChangesAsync(cancellationToken);
        }

        return new WishlistItemDto
        {
            ProductId = product.Id,
            PriceAtAdditionUah = existing.PriceAtAddition > 0 ? existing.PriceAtAddition : product.Price,
            CurrentPriceUah = product.Price,
            AddedAt = new DateTimeOffset(DateTime.SpecifyKind(existing.CreatedAt, DateTimeKind.Utc)),
            Product = new ProductCardDto
            {
                Id = product.Id,
                Title = product.Title,
                Slug = GenerateSlug(product.Title, product.Id),
                PrimaryImageUrl = product.Images.OrderBy(i => i.SortOrder).Select(i => i.FilePath).FirstOrDefault(),
                PriceUah = product.Price,
                Condition = product.Condition,
                IsMarketplace = product.IsMarketplace || product.Seller.RoleId == 1,
                IsInStock = product.StockQuantity > 0,
                StockQuantity = product.StockQuantity,
                Rating = (double)product.Rating,
                ReviewsCount = product.ReviewsCount,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                Seller = new ProductSellerSummaryDto
                {
                    Id = product.Seller.Id,
                    DisplayName = ResolveSellerDisplayName(product.IsMarketplace, product.Seller.RoleId, product.Seller.CompanyName, product.Seller.FullName),
                    SellerType = ResolveSellerType(product.IsMarketplace, product.Seller.RoleId, product.Seller.SellerType),
                    IsVerifiedSeller = product.IsMarketplace || product.Seller.RoleId == 1 || product.Seller.VerificationStatus == VerificationStatus.Approved,
                    Rating = (double)product.Seller.Rating,
                    ReviewsCount = product.ReviewsCount
                }
            }
        };
    }

    /// <summary>
    /// Removes a product from the authenticated buyer's wishlist.
    /// </summary>
    public async Task RemoveFromWishlistAsync(
        int productId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var wishlistItem = await db.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId, cancellationToken)
            ?? throw new NotFoundException($"Product with ID {productId} was not found in your wishlist.");

        db.Wishlists.Remove(wishlistItem);
        await db.SaveChangesAsync(cancellationToken);
    }

    // ─────────────────────── IOrderService ───────────────────────

    /// <summary>
    /// Creates orders grouped by seller from the buyer's current shopping cart and decrements product stock.
    /// </summary>
    public async Task<CheckoutOrdersResponse> CreateOrdersFromCartAsync(
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var buyer = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
            ?? throw new UnauthorizedException();

        var cartItems = await db.CartItems
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Seller)
            .Include(ci => ci.Product)
                .ThenInclude(p => p.Images)
            .Where(ci => ci.Cart.UserId == userId && !ci.Product.IsDeleted)
            .ToListAsync(cancellationToken);

        if (request.SellerIds is { Count: > 0 })
        {
            var sellerFilter = request.SellerIds.ToHashSet();
            cartItems = cartItems.Where(ci => sellerFilter.Contains(ci.Product.SellerId)).ToList();
        }

        if (cartItems.Count == 0)
        {
            throw new BadRequestException("Your shopping cart is empty.", "CART_EMPTY");
        }

        foreach (var item in cartItems)
        {
            if (item.Product.StockQuantity < item.Quantity)
            {
                throw new BadRequestException(
                    $"Insufficient stock for product '{item.Product.Title}'.",
                    "INSUFFICIENT_STOCK");
            }
        }

        var shippingAddress = $"{request.Shipping.City}, {request.Shipping.AddressOrBranch} ({request.Shipping.RecipientFullName}, {request.Shipping.RecipientPhone})";
        var createdOrders = new List<OrderDetailsResponse>();

        foreach (var sellerGroup in cartItems.GroupBy(ci => ci.Product.SellerId))
        {
            var seller = sellerGroup.First().Product.Seller;
            var groupTotal = sellerGroup.Sum(ci => ci.Product.Price * ci.Quantity);

            var order = new Order
            {
                UserId = userId,
                TotalPrice = groupTotal,
                ShippingAddress = shippingAddress.Length > 500 ? shippingAddress[..500] : shippingAddress,
                PaymentStatus = PaymentStatus.Pending,
                OrderStatus = OrderStatus.Created,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var ci in sellerGroup)
            {
                ci.Product.StockQuantity -= ci.Quantity;
                order.Items.Add(new OrderItem
                {
                    ProductId = ci.ProductId,
                    SellerId = ci.Product.SellerId,
                    Quantity = ci.Quantity,
                    PriceAtPurchase = ci.Product.Price,
                    OrderStatus = OrderStatus.Created
                });
            }

            db.Orders.Add(order);
            db.CartItems.RemoveRange(sellerGroup);
            await db.SaveChangesAsync(cancellationToken);

            createdOrders.Add(new OrderDetailsResponse
            {
                Id = order.Id,
                OrderNumber = $"ORD-{order.Id:D6}",
                BuyerId = userId,
                BuyerName = buyer.FullName,
                SellerId = seller.Id,
                SellerName = !string.IsNullOrWhiteSpace(seller.CompanyName) ? seller.CompanyName : seller.FullName,
                Status = order.OrderStatus,
                PaymentMethod = request.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                Shipping = request.Shipping,
                Items = sellerGroup.Select(ci => new OrderItemDto
                {
                    ProductId = ci.ProductId,
                    ProductTitle = ci.Product.Title,
                    ProductImageUrl = ci.Product.Images.OrderBy(i => i.SortOrder).Select(i => i.FilePath).FirstOrDefault(),
                    UnitPriceUah = ci.Product.Price,
                    Quantity = ci.Quantity
                }).ToList(),
                TotalAmountUah = groupTotal,
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc)),
                UpdatedAt = new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc))
            });
        }

        return new CheckoutOrdersResponse
        {
            Orders = createdOrders,
            GrandTotalUah = createdOrders.Sum(o => o.TotalAmountUah)
        };
    }

    public async Task<PagedResponse<OrderSummaryDto>> GetMyBuyerOrdersAsync(
        OrderFilterQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var ordersQuery = db.Orders
            .AsNoTracking()
            .Where(o => o.UserId == userId);

        if (query.Status.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderStatus == query.Status.Value);
        }

        var totalCount = await ordersQuery.CountAsync(cancellationToken);
        var items = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = $"ORD-{o.Id:D6}",
                BuyerId = o.UserId,
                BuyerName = o.User.FullName,
                SellerId = o.Items.Select(i => i.SellerId).FirstOrDefault(),
                SellerName = o.Items.Select(i => i.Seller.CompanyName ?? i.Seller.FullName).FirstOrDefault() ?? string.Empty,
                Status = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                DeliveryMethod = DeliveryMethod.Courier,
                TotalItemsCount = o.Items.Sum(i => i.Quantity),
                TotalAmountUah = o.TotalPrice,
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc))
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<OrderSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResponse<OrderSummaryDto>> GetMySellerOrdersAsync(
        OrderFilterQuery query,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();
        currentUser.EnsureOwner(userId);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var ordersQuery = db.Orders
            .AsNoTracking()
            .Where(o => o.Items.Any(i => i.SellerId == userId));

        if (query.Status.HasValue)
        {
            ordersQuery = ordersQuery.Where(o => o.OrderStatus == query.Status.Value);
        }

        var totalCount = await ordersQuery.CountAsync(cancellationToken);
        var items = await ordersQuery
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(o => new OrderSummaryDto
            {
                Id = o.Id,
                OrderNumber = $"ORD-{o.Id:D6}",
                BuyerId = o.UserId,
                BuyerName = o.User.FullName,
                SellerId = userId,
                SellerName = o.Items.Where(i => i.SellerId == userId).Select(i => i.Seller.CompanyName ?? i.Seller.FullName).FirstOrDefault() ?? string.Empty,
                Status = o.OrderStatus,
                PaymentStatus = o.PaymentStatus,
                DeliveryMethod = DeliveryMethod.Courier,
                TotalItemsCount = o.Items.Where(i => i.SellerId == userId).Sum(i => i.Quantity),
                TotalAmountUah = o.Items.Where(i => i.SellerId == userId).Sum(i => i.PriceAtPurchase * i.Quantity),
                CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(o.CreatedAt, DateTimeKind.Utc))
            })
            .ToListAsync(cancellationToken);

        return new PagedResponse<OrderSummaryDto>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderDetailsResponse> GetOrderByIdAsync(
        int orderId,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();

        var order = await db.Orders
            .AsNoTracking()
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                    .ThenInclude(p => p.Images)
            .Include(o => o.Items)
                .ThenInclude(i => i.Seller)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new NotFoundException($"Order with ID {orderId} was not found.");

        var isParticipant = order.UserId == userId || order.Items.Any(i => i.SellerId == userId) || currentUser.IsAdmin;
        if (!isParticipant)
        {
            throw new ForbiddenException("You do not have permission to view this order.");
        }

        var firstSeller = order.Items.FirstOrDefault()?.Seller;

        return new OrderDetailsResponse
        {
            Id = order.Id,
            OrderNumber = $"ORD-{order.Id:D6}",
            BuyerId = order.UserId,
            BuyerName = order.User.FullName,
            SellerId = firstSeller?.Id ?? 0,
            SellerName = firstSeller is not null ? (firstSeller.CompanyName ?? firstSeller.FullName) : string.Empty,
            Status = order.OrderStatus,
            PaymentMethod = PaymentMethod.TestCard,
            PaymentStatus = order.PaymentStatus,
            Shipping = new ShippingDetailsDto
            {
                RecipientFullName = order.User.FullName,
                RecipientPhone = order.User.PhoneNumber ?? string.Empty,
                DeliveryMethod = DeliveryMethod.Courier,
                City = string.Empty,
                AddressOrBranch = order.ShippingAddress
            },
            TrackingNumber = order.Items.Select(i => i.TrackingNumber).FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)),
            Items = order.Items.Select(i => new OrderItemDto
            {
                ProductId = i.ProductId,
                ProductTitle = i.Product.Title,
                ProductImageUrl = i.Product.Images.OrderBy(img => img.SortOrder).Select(img => img.FilePath).FirstOrDefault(),
                UnitPriceUah = i.PriceAtPurchase,
                Quantity = i.Quantity
            }).ToList(),
            TotalAmountUah = order.TotalPrice,
            CreatedAt = new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc)),
            UpdatedAt = new DateTimeOffset(DateTime.SpecifyKind(order.CreatedAt, DateTimeKind.Utc))
        };
    }

    public async Task<OrderDetailsResponse> UpdateOrderStatusAsync(
        int orderId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = RequireAuthenticatedUserId();

        var order = await db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken)
            ?? throw new NotFoundException($"Order with ID {orderId} was not found.");

        var isParticipant = order.UserId == userId || order.Items.Any(i => i.SellerId == userId) || currentUser.IsAdmin;
        if (!isParticipant)
        {
            throw new ForbiddenException("You do not have permission to update this order.");
        }

        order.OrderStatus = request.Status;
        foreach (var item in order.Items)
        {
            item.OrderStatus = request.Status;
            if (!string.IsNullOrWhiteSpace(request.TrackingNumber))
            {
                item.TrackingNumber = request.TrackingNumber.Trim();
            }
        }

        await db.SaveChangesAsync(cancellationToken);
        return await GetOrderByIdAsync(orderId, cancellationToken);
    }

    // ─────────────────────── Helpers ───────────────────────

    private int RequireAuthenticatedUserId()
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            throw new UnauthorizedException();
        }

        return currentUser.UserId.Value;
    }

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
