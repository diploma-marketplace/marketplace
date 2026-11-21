using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FutureShopBackend.Domain.Entities
{
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
        public SellerType? SellerType { get; set; }
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
}