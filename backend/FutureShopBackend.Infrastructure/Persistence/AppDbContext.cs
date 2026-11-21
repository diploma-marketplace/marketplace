using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FutureShopBackend.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductAttribute> Attributes => Set<ProductAttribute>();
    public DbSet<ProductAttributeValue> ProductAttributeValues => Set<ProductAttributeValue>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> Wishlists => Set<WishlistItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<Review> Reviews => Set<Review>();

    // Enum -> рядок у нижньому регістрі ("unpaid", "fop", ...)
    private static ValueConverter<TEnum, string> Lower<TEnum>() where TEnum : struct, Enum
        => new(v => v.ToString().ToLowerInvariant(), s => Enum.Parse<TEnum>(s, true));

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b); // таблиці Identity (asp_net_*), імена в snake_case дає UseSnakeCaseNamingConvention()

        // ───────── ROLES (+ seed: admin, company, user) ─────────
        b.Entity<ApplicationRole>(e =>
        {
            e.HasData(
                new ApplicationRole { Id = 1, Name = "admin", NormalizedName = "ADMIN", ConcurrencyStamp = "role-admin-v1" },
                new ApplicationRole { Id = 2, Name = "company", NormalizedName = "COMPANY", ConcurrencyStamp = "role-company-v1" },
                new ApplicationRole { Id = 3, Name = "user", NormalizedName = "USER", ConcurrencyStamp = "role-user-v1" });
        });

        // ───────── USERS ─────────
        b.Entity<ApplicationUser>(e =>
        {
            e.Property(u => u.UserName).IsRequired();
            e.Property(u => u.Email).IsRequired();
            e.Property(u => u.PasswordHash).IsRequired();
            e.Property(u => u.FullName).IsRequired().HasMaxLength(200);
            e.Property(u => u.AvatarUrl).HasMaxLength(500);
            e.Property(u => u.SellerType).HasConversion(Lower<SellerType>()).HasMaxLength(20);
            e.Property(u => u.Rating).HasPrecision(3, 2).HasDefaultValue(0m);
            e.Property(u => u.CreatedAt).HasDefaultValueSql("now()");

            // email UNIQUE (в Identity за замовчуванням індекс по NormalizedEmail не унікальний)
            e.HasIndex(u => u.NormalizedEmail).IsUnique();

            e.HasOne(u => u.Role).WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        // ───────── CATEGORIES ─────────
        b.Entity<Category>(e =>
        {
            e.ToTable("categories");
            e.Property(c => c.Name).IsRequired().HasMaxLength(150);
            e.Property(c => c.Slug).IsRequired().HasMaxLength(200);
            e.HasIndex(c => c.Slug).IsUnique();

            e.HasOne(c => c.Parent).WithMany(c => c.Children)
                .HasForeignKey(c => c.ParentId).OnDelete(DeleteBehavior.Restrict);
        });

        // ───────── PRODUCTS ─────────
        b.Entity<Product>(e =>
        {
            e.ToTable("products", t =>
            {
                t.HasCheckConstraint("ck_products_price_positive", "price > 0");
                t.HasCheckConstraint("ck_products_stock_non_negative", "stock_quantity >= 0");
            });

            e.Property(p => p.Title).IsRequired().HasMaxLength(300);
            e.Property(p => p.Description).HasColumnType("text");
            e.Property(p => p.Price).HasPrecision(12, 2);
            e.Property(p => p.IsMarketplace).HasDefaultValue(false);
            e.Property(p => p.IsDeleted).HasDefaultValue(false);
            e.Property(p => p.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(p => p.Seller).WithMany(u => u.Products)
                .HasForeignKey(p => p.SellerId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(p => p.Category).WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId).OnDelete(DeleteBehavior.Restrict);
        });

        // ───────── PRODUCT_IMAGES ─────────
        b.Entity<ProductImage>(e =>
        {
            e.ToTable("product_images");
            e.Property(i => i.FilePath).IsRequired().HasMaxLength(500);
            e.Property(i => i.SortOrder).HasDefaultValue(0);

            e.HasOne(i => i.Product).WithMany(p => p.Images)
                .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // ───────── ATTRIBUTES ─────────
        b.Entity<ProductAttribute>(e =>
        {
            e.ToTable("attributes");
            e.Property(a => a.Name).IsRequired().HasMaxLength(150);

            e.HasOne(a => a.Category).WithMany(c => c.Attributes)
                .HasForeignKey(a => a.CategoryId).OnDelete(DeleteBehavior.Cascade);
        });

        // ───────── PRODUCT_ATTRIBUTE_VALUES ─────────
        b.Entity<ProductAttributeValue>(e =>
        {
            e.ToTable("product_attribute_values");
            e.Property(v => v.Value).IsRequired().HasMaxLength(500);
            e.HasIndex(v => new { v.ProductId, v.AttributeId }).IsUnique();

            e.HasOne(v => v.Product).WithMany(p => p.AttributeValues)
                .HasForeignKey(v => v.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Attribute).WithMany(a => a.Values)
                .HasForeignKey(v => v.AttributeId).OnDelete(DeleteBehavior.Restrict);
        });

        // ───────── CARTS ─────────
        b.Entity<Cart>(e =>
        {
            e.ToTable("carts");
            e.HasIndex(c => c.UserId).IsUnique();

            e.HasOne(c => c.User).WithOne(u => u.Cart)
                .HasForeignKey<Cart>(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        // ───────── CART_ITEMS ─────────
        b.Entity<CartItem>(e =>
        {
            e.ToTable("cart_items", t => t.HasCheckConstraint("ck_cart_items_quantity_positive", "quantity > 0"));
            e.HasIndex(i => new { i.CartId, i.ProductId }).IsUnique();

            e.HasOne(i => i.Cart).WithMany(c => c.Items)
                .HasForeignKey(i => i.CartId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Product).WithMany()
                .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // ───────── WISHLISTS ─────────
        b.Entity<WishlistItem>(e =>
        {
            e.ToTable("wishlists");
            e.HasIndex(w => new { w.UserId, w.ProductId }).IsUnique();

            e.HasOne(w => w.User).WithMany(u => u.WishlistItems)
                .HasForeignKey(w => w.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(w => w.Product).WithMany()
                .HasForeignKey(w => w.ProductId).OnDelete(DeleteBehavior.Cascade);
        });

        // ───────── ORDERS ─────────
        b.Entity<Order>(e =>
        {
            e.ToTable("orders", t => t.HasCheckConstraint("ck_orders_total_price_non_negative", "total_price >= 0"));
            e.Property(o => o.TotalPrice).HasPrecision(12, 2);
            e.Property(o => o.ShippingAddress).IsRequired().HasMaxLength(500);
            e.Property(o => o.PaymentStatus).HasConversion(Lower<PaymentStatus>()).HasMaxLength(20)
                .HasDefaultValue(PaymentStatus.Pending);
            e.Property(o => o.OrderStatus).HasConversion(Lower<OrderStatus>()).HasMaxLength(20)
                .HasDefaultValue(OrderStatus.Created);
            e.Property(o => o.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(o => o.User).WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        // ───────── ORDER_ITEMS ─────────
        b.Entity<OrderItem>(e =>
        {
            e.ToTable("order_items", t =>
            {
                t.HasCheckConstraint("ck_order_items_quantity_positive", "quantity > 0");
                t.HasCheckConstraint("ck_order_items_price_positive", "price_at_purchase > 0");
            });
            e.Property(i => i.PriceAtPurchase).HasPrecision(12, 2);
            e.Property(i => i.OrderStatus).HasConversion(Lower<OrderStatus>()).HasMaxLength(20)
                .HasDefaultValue(OrderStatus.Created);
            e.Property(i => i.TrackingNumber).HasMaxLength(100);

            e.HasOne(i => i.Order).WithMany(o => o.Items)
                .HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Product).WithMany()
                .HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(i => i.Seller).WithMany(u => u.SoldOrderItems)
                .HasForeignKey(i => i.SellerId).OnDelete(DeleteBehavior.Restrict);
        });

        // ───────── REVIEWS ─────────
        b.Entity<Review>(e =>
        {
            e.ToTable("reviews", t =>
            {
                t.HasCheckConstraint("ck_reviews_rating_range", "rating BETWEEN 1 AND 5");
                t.HasCheckConstraint("ck_reviews_single_target", "num_nonnulls(product_id, seller_id) = 1");
            });
            e.Property(r => r.Comment).HasColumnType("text");
            e.Property(r => r.CreatedAt).HasDefaultValueSql("now()");

            e.HasOne(r => r.User).WithMany(u => u.WrittenReviews)
                .HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Product).WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(r => r.Seller).WithMany(u => u.ReceivedReviews)
                .HasForeignKey(r => r.SellerId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
