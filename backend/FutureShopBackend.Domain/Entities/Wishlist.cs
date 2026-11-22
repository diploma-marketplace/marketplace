namespace FutureShopBackend.Domain.Entities
{
    /// <summary>
    /// Represents a product saved to a buyer's wishlist with price-at-addition tracking.
    /// </summary>
    public class WishlistItem
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public ApplicationUser User { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public decimal PriceAtAddition { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
