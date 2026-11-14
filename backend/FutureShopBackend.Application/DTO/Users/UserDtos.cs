using System.ComponentModel.DataAnnotations;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.DTO.Users;

/// <summary>
/// Social media links associated with a user or seller profile.
/// </summary>
public sealed record SocialLinksDto
{
    /// <summary>Telegram handle or link.</summary>
    [StringLength(200)]
    public string? Telegram { get; init; }

    /// <summary>Instagram profile link.</summary>
    [StringLength(200)]
    public string? Instagram { get; init; }

    /// <summary>Facebook page or profile link.</summary>
    [StringLength(200)]
    public string? Facebook { get; init; }
}

/// <summary>
/// Full private profile response for the currently authenticated user or company.
/// </summary>
public sealed record UserProfileResponse
{
    /// <summary>Unique user identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Account email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Whether the email address has been verified.</summary>
    public bool IsEmailVerified { get; init; }

    /// <summary>Full name or display nickname.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Contact phone number.</summary>
    public string? PhoneNumber { get; init; }

    /// <summary>Whether the phone number has been verified.</summary>
    public bool IsPhoneVerified { get; init; }

    /// <summary>Profile avatar URL.</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>Primary RBAC role (User, Company, Admin).</summary>
    public UserRole Role { get; init; }

    /// <summary>Current account status.</summary>
    public AccountStatus AccountStatus { get; init; }

    /// <summary>Seller verification status (for FOP / Company).</summary>
    public VerificationStatus VerificationStatus { get; init; }

    /// <summary>Indicates whether the user is a sole proprietor (FOP).</summary>
    public bool IsSoleProprietor { get; init; }

    /// <summary>Company or store name (for FOP and Company sellers).</summary>
    public string? CompanyName { get; init; }

    /// <summary>EDRPOU or RNOKPP tax code (visible only to account owner and admins).</summary>
    public string? TaxRegistrationCode { get; init; }

    /// <summary>Official legal or return address.</summary>
    public string? Address { get; init; }

    /// <summary>External company website URL.</summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>Social media links.</summary>
    public SocialLinksDto? SocialLinks { get; init; }

    /// <summary>Average seller rating (1.0 - 5.0).</summary>
    public double Rating { get; init; }

    /// <summary>Total number of received reviews.</summary>
    public int ReviewsCount { get; init; }

    /// <summary>Number of currently active product listings.</summary>
    public int ActiveListingsCount { get; init; }

    /// <summary>Maximum allowed active product listings for this account tier.</summary>
    public int MaxActiveListingsLimit { get; init; }

    /// <summary>Preferred UI language (Uk, En).</summary>
    public InterfaceLanguage PreferredLanguage { get; init; }

    /// <summary>Preferred UI theme (Light, Dark, System).</summary>
    public ThemeMode PreferredTheme { get; init; }

    /// <summary>Account creation timestamp in UTC.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}

/// <summary>
/// Request payload for updating the authenticated user's profile and UI preferences.
/// </summary>
public sealed record UpdateProfileRequest
{
    /// <summary>Full name or display nickname.</summary>
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public required string FullName { get; init; }

    /// <summary>Contact phone number.</summary>
    [Phone]
    [StringLength(32)]
    public string? PhoneNumber { get; init; }

    /// <summary>Avatar image URL.</summary>
    [Url]
    [StringLength(500)]
    public string? AvatarUrl { get; init; }

    /// <summary>Public store or company display name.</summary>
    [StringLength(200)]
    public string? CompanyName { get; init; }

    /// <summary>Contact or return address.</summary>
    [StringLength(300)]
    public string? Address { get; init; }

    /// <summary>External website URL (for companies).</summary>
    [Url]
    [StringLength(500)]
    public string? WebsiteUrl { get; init; }

    /// <summary>Social media links.</summary>
    public SocialLinksDto? SocialLinks { get; init; }

    /// <summary>Preferred UI localization language.</summary>
    public InterfaceLanguage PreferredLanguage { get; init; } = InterfaceLanguage.Uk;

    /// <summary>Preferred UI color theme.</summary>
    public ThemeMode PreferredTheme { get; init; } = ThemeMode.System;
}

/// <summary>
/// Request payload for changing the authenticated user's password.
/// </summary>
public sealed record ChangePasswordRequest
{
    /// <summary>Current password.</summary>
    [Required]
    public required string CurrentPassword { get; init; }

    /// <summary>New password (minimum 8 characters).</summary>
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public required string NewPassword { get; init; }
}

/// <summary>
/// Request payload for submitting FOP (sole proprietor) or Company verification credentials.
/// </summary>
public sealed record SubmitSellerVerificationRequest
{
    /// <summary>Whether the applicant is a sole proprietor (FOP) or a legal entity (Company).</summary>
    public bool IsSoleProprietor { get; init; }

    /// <summary>Registered business or company name.</summary>
    [Required]
    [StringLength(200)]
    public required string LegalEntityName { get; init; }

    /// <summary>RNOKPP (for FOP) or EDRPOU (for Company) registration number.</summary>
    [Required]
    [StringLength(16, MinimumLength = 8)]
    public required string TaxIdentificationCode { get; init; }

    /// <summary>Registered legal address.</summary>
    [Required]
    [StringLength(300)]
    public required string LegalAddress { get; init; }

    /// <summary>IBAN bank account for payouts.</summary>
    [Required]
    [StringLength(34, MinimumLength = 15)]
    public required string BankIban { get; init; }

    /// <summary>Additional notes or links to registration documents.</summary>
    [StringLength(1000)]
    public string? SupportingDocumentNotes { get; init; }
}

/// <summary>
/// Response describing a seller verification application.
/// </summary>
public sealed record SellerVerificationResponse
{
    /// <summary>Verification request identifier.</summary>
    public Guid VerificationId { get; init; }

    /// <summary>Applicant user identifier.</summary>
    public Guid UserId { get; init; }

    /// <summary>Registered legal entity or FOP name.</summary>
    public string LegalEntityName { get; init; } = string.Empty;

    /// <summary>Tax identification code (RNOKPP or EDRPOU).</summary>
    public string TaxIdentificationCode { get; init; } = string.Empty;

    /// <summary>Current moderation status of the verification request.</summary>
    public VerificationStatus Status { get; init; }

    /// <summary>Moderator comment or rejection reason (if applicable).</summary>
    public string? ReviewerComment { get; init; }

    /// <summary>Submission timestamp in UTC.</summary>
    public DateTimeOffset SubmittedAt { get; init; }
}

/// <summary>
/// Public seller profile displayed to buyers on product and seller pages.
/// </summary>
public sealed record PublicSellerProfileResponse
{
    /// <summary>Seller identifier.</summary>
    public Guid Id { get; init; }

    /// <summary>Display name or company title.</summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>Profile avatar or company logo URL.</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>Role of the seller (User, Company, Admin/Marketplace).</summary>
    public UserRole Role { get; init; }

    /// <summary>Type of seller (Marketplace vs ThirdParty).</summary>
    public SellerType SellerType { get; init; }

    /// <summary>Whether the seller is an officially verified FOP or Company.</summary>
    public bool IsVerifiedSeller { get; init; }

    /// <summary>Whether the seller operates as a sole proprietor (FOP).</summary>
    public bool IsSoleProprietor { get; init; }

    /// <summary>Average seller rating (1.0 - 5.0) displayed next to nickname.</summary>
    public double Rating { get; init; }

    /// <summary>Total number of reviews received by the seller.</summary>
    public int ReviewsCount { get; init; }

    /// <summary>Public contact email.</summary>
    public string? ContactEmail { get; init; }

    /// <summary>Public contact phone number.</summary>
    public string? ContactPhone { get; init; }

    /// <summary>External website URL (for Company sellers).</summary>
    public string? WebsiteUrl { get; init; }

    /// <summary>Social media links.</summary>
    public SocialLinksDto? SocialLinks { get; init; }

    /// <summary>Number of active products currently listed by the seller.</summary>
    public int ActiveProductsCount { get; init; }

    /// <summary>Date when the seller joined the marketplace.</summary>
    public DateTimeOffset JoinedAt { get; init; }
}

/// <summary>
/// Analytics summary for sellers and companies (views, favorites, sales, conversion, revenue).
/// </summary>
public sealed record SellerAnalyticsResponse
{
    /// <summary>Total active listings count.</summary>
    public int ActiveListingsCount { get; init; }

    /// <summary>Total product page views across all listings.</summary>
    public long TotalProductViews { get; init; }

    /// <summary>Total times seller's products were added to wishlists.</summary>
    public int TotalWishlistAdditions { get; init; }

    /// <summary>Total completed orders count.</summary>
    public int CompletedOrdersCount { get; init; }

    /// <summary>Conversion rate percentage (orders / views).</summary>
    public decimal ConversionRatePercent { get; init; }

    /// <summary>Total gross revenue in UAH.</summary>
    public decimal TotalRevenueUah { get; init; }

    /// <summary>Average seller rating.</summary>
    public double AverageRating { get; init; }
}
