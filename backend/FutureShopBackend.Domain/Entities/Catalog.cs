using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Domain.Entities
{
    /// <summary>
    /// Hierarchical product category supporting dynamic specification attributes.
    /// </summary>
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

    /// <summary>
    /// Product listing offered either by the first-party Marketplace or by a third-party seller.
    /// </summary>
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
        public ProductCondition Condition { get; set; } = ProductCondition.New;
        public bool IsMarketplace { get; set; }
        public decimal Rating { get; set; }
        public int ReviewsCount { get; set; }
        public bool IsDeleted { get; set; } // Soft delete flag
        public DateTime CreatedAt { get; set; }

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public ICollection<ProductAttributeValue> AttributeValues { get; set; } = new List<ProductAttributeValue>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }

    /// <summary>
    /// Product gallery image ordered by SortOrder.
    /// </summary>
    public class ProductImage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public string FilePath { get; set; } = string.Empty; // Relative path or URL served via Nginx
        public int SortOrder { get; set; }
    }

    // Named ProductAttribute (instead of Attribute) to avoid conflict with System.Attribute.
    // Mapped to the "attributes" table in PostgreSQL.
    public class ProductAttribute
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string Name { get; set; } = string.Empty;

        public ICollection<ProductAttributeValue> Values { get; set; } = new List<ProductAttributeValue>();
    }

    /// <summary>
    /// Specification value assigned to a product for a specific category attribute.
    /// </summary>
    public class ProductAttributeValue
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int AttributeId { get; set; }
        public ProductAttribute Attribute { get; set; } = null!;

        public string Value { get; set; } = string.Empty;
    }
}