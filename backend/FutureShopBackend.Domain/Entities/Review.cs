namespace FutureShopBackend.Domain.Entities
{
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
        public DateTime CreatedAt { get; set; }
    }
}