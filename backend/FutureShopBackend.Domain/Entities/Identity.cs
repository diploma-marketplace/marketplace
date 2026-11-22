using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FutureShopBackend.Domain.Entities
{
    /// <summary>
    /// Represents a system role in ASP.NET Core Identity.
    /// </summary>
    public class ApplicationRole : IdentityRole<int>
    {
        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }

    /// <summary>
    /// Represents an authenticated marketplace user, sole proprietor (FOP), company seller, or administrator.
    /// </summary>
    public class ApplicationUser : IdentityUser<int>
    {
        public int RoleId { get; set; }
        public ApplicationRole Role { get; set; } = null!;

        public string FullName { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
        public SellerType? SellerType { get; set; }
        public VerificationStatus VerificationStatus { get; set; } = VerificationStatus.NotSubmitted;
        public string? CompanyName { get; set; }
        public string? TaxIdentificationCode { get; set; }
        public string? Address { get; set; }
        public string? BankIban { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? TelegramUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? FacebookUrl { get; set; }
        public DateTime? VerificationSubmittedAt { get; set; }
        public InterfaceLanguage PreferredLanguage { get; set; } = InterfaceLanguage.Uk;
        public ThemeMode PreferredTheme { get; set; } = ThemeMode.System;
        public decimal Rating { get; set; }
        public DateTime CreatedAt { get; set; } // Always stored in UTC

        public Cart? Cart { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();          // Listed as seller
        public ICollection<WishlistItem> WishlistItems { get; set; } = new List<WishlistItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();                // Placed as buyer
        public ICollection<OrderItem> SoldOrderItems { get; set; } = new List<OrderItem>(); // Sold as seller
        public ICollection<Review> WrittenReviews { get; set; } = new List<Review>();
        public ICollection<Review> ReceivedReviews { get; set; } = new List<Review>();     // Reviews received as seller
    }
}