using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Domain.Entities
{
    /// <summary>
    /// Represents a buyer's order placed on the marketplace.
    /// </summary>
    public class Order
    {
        public int Id { get; set; }

        public int UserId { get; set; } // Buyer identifier
        public ApplicationUser User { get; set; } = null!;

        public decimal TotalPrice { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public PaymentStatus PaymentStatus { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }

    /// <summary>
    /// Represents an individual line item within an order fulfilled by a specific seller.
    /// </summary>
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int SellerId { get; set; } // Seller fulfilling this line item
        public ApplicationUser Seller { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? TrackingNumber { get; set; } // Carrier tracking number from seller
    }
}
