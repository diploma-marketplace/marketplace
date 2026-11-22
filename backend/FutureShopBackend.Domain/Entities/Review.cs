namespace FutureShopBackend.Domain.Entities
{
    /// <summary>
    /// Represents a verified buyer review and star rating (1..5) for a product and/or seller.
    /// </summary>
    public class Review
    {
        public int Id { get; set; }

        public int UserId { get; set; } // Author of the review
        public ApplicationUser User { get; set; } = null!;

        public int? ProductId { get; set; }
        public Product? Product { get; set; }

        public int? SellerId { get; set; }
        public ApplicationUser? Seller { get; set; }

        public int Rating { get; set; } // 1..5
        public string? Comment { get; set; }
        public string? Pros { get; set; }
        public string? Cons { get; set; }
        public string? SellerReply { get; set; }
        public DateTime? SellerRepliedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}