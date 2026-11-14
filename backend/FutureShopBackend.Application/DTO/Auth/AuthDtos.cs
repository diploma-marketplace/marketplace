using System.ComponentModel.DataAnnotations;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Application.DTO.Auth;

/// <summary>
/// Request payload for registering a new user or company account.
/// </summary>
public sealed record RegisterRequest
{
    /// <summary>Account email address.</summary>
    /// <example>user@futuremarket.ua</example>
    [Required]
    [EmailAddress]
    [StringLength(256)]
    public required string Email { get; init; }

    /// <summary>Account password (hashed on server using PBKDF2/Argon2/bcrypt).</summary>
    /// <example>StrongPass!2026</example>
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public required string Password { get; init; }

    /// <summary>Full name or display nickname.</summary>
    /// <example>Oleksandr Kovalenko</example>
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public required string FullName { get; init; }

    /// <summary>Contact phone number.</summary>
    /// <example>+380501234567</example>
    [Phone]
    [StringLength(32)]
    public string? PhoneNumber { get; init; }

    /// <summary>Selected account role at registration (User or Company).</summary>
    /// <example>User</example>
    [Required]
    public UserRole Role { get; init; } = UserRole.User;

    /// <summary>Company details (required when registering with Role = Company).</summary>
    public CompanyRegistrationDetails? CompanyDetails { get; init; }
}

/// <summary>
/// Corporate registration details required for legal entity (Company) accounts.
/// </summary>
public sealed record CompanyRegistrationDetails
{
    /// <summary>Official registered company name.</summary>
    /// <example>TechStore Ukraine LLC</example>
    [Required]
    [StringLength(200)]
    public required string CompanyName { get; init; }

    /// <summary>State registration code (EDRPOU).</summary>
    /// <example>12345678</example>
    [Required]
    [StringLength(16, MinimumLength = 8)]
    public required string EdrpouCode { get; init; }

    /// <summary>Official legal address of the company.</summary>
    /// <example>01001, Kyiv, Khreshchatyk St, 1</example>
    [Required]
    [StringLength(300)]
    public required string LegalAddress { get; init; }

    /// <summary>Authorized contact person name.</summary>
    /// <example>Iryna Melnyk</example>
    [Required]
    [StringLength(150)]
    public required string ContactPersonName { get; init; }

    /// <summary>External company website URL.</summary>
    /// <example>https://techstore.example.ua</example>
    [Url]
    [StringLength(500)]
    public string? WebsiteUrl { get; init; }
}

/// <summary>
/// Request payload for session cookie login.
/// </summary>
public sealed record LoginRequest
{
    /// <summary>Registered email address.</summary>
    /// <example>user@futuremarket.ua</example>
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    /// <summary>Account password.</summary>
    /// <example>StrongPass!2026</example>
    [Required]
    public required string Password { get; init; }

    /// <summary>Whether to persist the session cookie across browser restarts.</summary>
    public bool RememberMe { get; init; }
}

/// <summary>
/// Request payload for confirming email or phone verification code.
/// </summary>
public sealed record VerifyContactRequest
{
    /// <summary>Verification target (email or phone).</summary>
    /// <example>email</example>
    [Required]
    [StringLength(32)]
    public required string Channel { get; init; }

    /// <summary>One-time confirmation code sent to the user.</summary>
    /// <example>482910</example>
    [Required]
    [StringLength(16, MinimumLength = 4)]
    public required string Code { get; init; }
}

/// <summary>
/// Request payload for initiating a password reset flow.
/// </summary>
public sealed record ForgotPasswordRequest
{
    /// <summary>Account email address to receive the reset link/code.</summary>
    /// <example>user@futuremarket.ua</example>
    [Required]
    [EmailAddress]
    public required string Email { get; init; }
}

/// <summary>
/// Request payload for completing a password reset.
/// </summary>
public sealed record ResetPasswordRequest
{
    /// <summary>Account email address.</summary>
    [Required]
    [EmailAddress]
    public required string Email { get; init; }

    /// <summary>Password reset token received via email.</summary>
    [Required]
    public required string ResetToken { get; init; }

    /// <summary>New account password.</summary>
    [Required]
    [StringLength(128, MinimumLength = 8)]
    public required string NewPassword { get; init; }
}

/// <summary>
/// Authenticated session summary returned after login, registration, or session check.
/// </summary>
public sealed record AuthSessionResponse
{
    /// <summary>Unique user identifier.</summary>
    public Guid UserId { get; init; }

    /// <summary>Account email address.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Full name or display nickname.</summary>
    public string FullName { get; init; } = string.Empty;

    /// <summary>Avatar image URL.</summary>
    public string? AvatarUrl { get; init; }

    /// <summary>Primary RBAC role (User, Company, Admin).</summary>
    public UserRole Role { get; init; }

    /// <summary>Current account lifecycle status.</summary>
    public AccountStatus AccountStatus { get; init; }

    /// <summary>Seller verification status (for FOP and Company sellers).</summary>
    public VerificationStatus VerificationStatus { get; init; }

    /// <summary>Preferred UI language.</summary>
    public InterfaceLanguage PreferredLanguage { get; init; } = InterfaceLanguage.Uk;

    /// <summary>Preferred UI theme mode.</summary>
    public ThemeMode PreferredTheme { get; init; } = ThemeMode.System;

    /// <summary>Session expiration timestamp in UTC.</summary>
    public DateTimeOffset ExpiresAt { get; init; }
}
