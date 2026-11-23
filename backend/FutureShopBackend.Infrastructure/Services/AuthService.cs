using FutureShopBackend.Application.DTO.Auth;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FutureShopBackend.Infrastructure.Services;

/// <summary>
/// Handles account registration, authentication, and session profile lookups.
/// Pure business logic decoupled from HTTP, cookies, and request contexts.
/// </summary>
public sealed class AuthService(
    UserManager<ApplicationUser> users,
    ICurrentUserService currentUser) : IAuthService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RememberMeLifetime = TimeSpan.FromDays(14);

    public async Task<AuthSessionResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        if (request.Role == UserRole.Admin)
            throw new ForbiddenException("Admin accounts cannot be self-registered.");
        if (request.Role == UserRole.Company && request.CompanyDetails is null)
            throw new BadRequestException("Company details are required for company accounts.", "VALIDATION_ERROR");

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            FullName = request.FullName,
            RoleId = request.Role switch
            {
                UserRole.Company => 2,
                UserRole.Guest => 4,
                _ => 3
            },
            SellerType = request.Role == UserRole.Company ? SellerType.Company : null,
            CompanyName = request.CompanyDetails?.CompanyName,
            TaxIdentificationCode = request.CompanyDetails?.EdrpouCode,
            Address = request.CompanyDetails?.LegalAddress,
            WebsiteUrl = request.CompanyDetails?.WebsiteUrl,
            VerificationStatus = request.Role == UserRole.Company && request.CompanyDetails is not null
                ? VerificationStatus.Pending
                : VerificationStatus.NotSubmitted,
            VerificationSubmittedAt = request.Role == UserRole.Company && request.CompanyDetails is not null
                ? DateTime.UtcNow
                : null,
            CreatedAt = DateTime.UtcNow
        };

        var result = await users.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
                throw new ConflictException("An account with this email already exists.", "EMAIL_TAKEN");

            throw new BadRequestException(
                "Registration failed.", "VALIDATION_ERROR",
                new Dictionary<string, string[]> { ["password"] = result.Errors.Select(e => e.Description).ToArray() });
        }

        var expires = DateTimeOffset.UtcNow.Add(SessionLifetime);
        return ToResponse(user, request.Role, expires);
    }

    public async Task<AuthSessionResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.FindByEmailAsync(request.Email);

        // Return an identical error for unknown email and invalid password to prevent user enumeration.
        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid email or password.", "INVALID_CREDENTIALS");

        if (await users.IsLockedOutAsync(user))
            throw new ForbiddenException("Account is temporarily blocked.", "ACCOUNT_BLOCKED");

        var role = user.RoleId switch
        {
            1 => UserRole.Admin,
            2 => UserRole.Company,
            4 => UserRole.Guest,
            _ => UserRole.User
        };
        var expires = DateTimeOffset.UtcNow.Add(request.RememberMe ? RememberMeLifetime : SessionLifetime);

        return ToResponse(user, role, expires);
    }

    public async Task<AuthSessionResponse> GetCurrentSessionAsync(int userId, CancellationToken ct = default)
    {
        // The requested account must belong to the authenticated server-side session.
        // The owner id is never trusted from a request body or route.
        currentUser.EnsureOwner(userId);

        var user = await users.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException();

        var role = user.RoleId switch
        {
            1 => UserRole.Admin,
            2 => UserRole.Company,
            4 => UserRole.Guest,
            _ => UserRole.User
        };
        return ToResponse(user, role, DateTimeOffset.UtcNow.Add(SessionLifetime));
    }

    public Task<MessageResponse> VerifyContactAsync(VerifyContactRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Contact verification is not part of this sprint.");

    public Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Password recovery is not part of this sprint.");

    public Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Password recovery is not part of this sprint.");

    private static AuthSessionResponse ToResponse(ApplicationUser user, UserRole role, DateTimeOffset expiresAt) => new()
    {
        UserId = user.Id,
        Email = user.Email ?? string.Empty,
        FullName = user.FullName,
        AvatarUrl = user.AvatarUrl,
        Role = role,
        AccountStatus = AccountStatus.Active,
        VerificationStatus = user.VerificationStatus,
        PreferredLanguage = user.PreferredLanguage,
        PreferredTheme = user.PreferredTheme,
        ExpiresAt = expiresAt
    };
}
