using Microsoft.AspNetCore.Identity;

namespace FutureShopBackend.Domain.Entities;

// ───────────── Identity ─────────────

public class ApplicationRole : IdentityRole<int>
{
    public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}

public class ApplicationUser : IdentityUser<int>
{
    public int RoleId { get; set; }
    public ApplicationRole Role { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public SellerStatusType? StatusType { get; set; }
    public decimal Rating { get; set; }
    public DateTime CreatedAt { get; set; } // завжди UTC

    public Cart? Cart { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();          // як продавець
    public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();                // як покупець
    public ICollection<OrderItem> SoldOrderItems { get; set; } = new List<OrderItem>(); // як продавець позиції
    public ICollection<Review> WrittenReviews { get; set; } = new List<Review>();
    public ICollection<Review> ReceivedReviews { get; set; } = new List<Review>();     // відгуки про продавця
}

// ───────────── Каталог ─────────────

public class Category
{
    public int Id { get; set; }
    public int? ParentId { get; set; }
    public Category? Parent { get; set; }
    public ICollection<Category> Children { get; set; } = new List<Category>();

    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
    public ICollection<ProductAttribute> Attributes { get; set; } = new List<ProductAttribute>();
}

public class Product
{
    public int Id { get; set; }

    public int SellerId { get; set; }
    public ApplicationUser Seller { get; set; } = null!;

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public bool IsMarketplace { get; set; }
    public bool IsDeleted { get; set; } // soft delete
    public DateTime CreatedAt { get; set; }

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}

public class ProductImage
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public string FilePath { get; set; } = string.Empty; // шлях для Nginx
    public int SortOrder { get; set; }
}

// Назва ProductAttribute (а не Attribute), щоб не конфліктувати з System.Attribute.
// Таблиця в БД називається "attributes".
public class ProductAttribute
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
}

public class ProductAttributeValue
{
    public int Id { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int AttributeId { get; set; }
    public ProductAttribute Attribute { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}

// ───────────── Кошик та вішліст ─────────────

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
}

public class CartItem
{
    public int Id { get; set; }

    public int CartId { get; set; }
    public Cart Cart { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}

public class WishlistItem
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}

// ───────────── Замовлення ─────────────

public class Order
{
    public int Id { get; set; }

    public int UserId { get; set; } // покупець
    public ApplicationUser User { get; set; } = null!;

    public decimal TotalPrice { get; set; }
    public string ShippingAddress { get; set; } = string.Empty;
    public PaymentStatus PaymentStatus { get; set; }
    public OrderStatus OrderStatus { get; set; }
    public DateTime CreatedAt { get; set; }

    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}

public class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public Order Order { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int SellerId { get; set; } // продавець позиції
    public ApplicationUser Seller { get; set; } = null!;

    public int Quantity { get; set; }
    public decimal PriceAtPurchase { get; set; }
    public OrderItemStatus ItemStatus { get; set; }
    public string? TrackingNumber { get; set; } // ТТН від конкретного продавця
}

// ───────────── Відгуки ─────────────

public class Review
{
    public int Id { get; set; }

    public int UserId { get; set; } // автор
    public ApplicationUser User { get; set; } = null!;

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    public int? SellerId { get; set; }
    public ApplicationUser? Seller { get; set; }

    public int Rating { get; set; } // 1..5
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}
