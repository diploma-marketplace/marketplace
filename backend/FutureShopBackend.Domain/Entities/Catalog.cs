namespace FutureShopBackend.Domain.Entities
{
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
}