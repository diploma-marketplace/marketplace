using FutureShopBackend.Application.DTO.Auth;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Identity;

namespace FutureShopBackend.Application.Services;

/// <summary>
/// Реєстрація, вхід і профіль сесії.
/// Чиста бізнес-логіка, яка не знає про HTTP, куки чи контексти запиту.
/// </summary>
public sealed class AuthService(UserManager<ApplicationUser> users) : IAuthService
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
            RoleId = request.Role == UserRole.Company ? 2 : 3, // id з seed-даних AppDbContext
            SellerType = request.Role == UserRole.Company ? SellerType.Company : null,
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

        // Однакова відповідь для "немає користувача" і "поганий пароль" — не розкриваємо, які email існують.
        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid email or password.", "INVALID_CREDENTIALS");

        if (await users.IsLockedOutAsync(user))
            throw new ForbiddenException("Account is temporarily blocked.", "ACCOUNT_BLOCKED");

        var role = user.RoleId == 2 ? UserRole.Company : (user.RoleId == 1 ? UserRole.Admin : UserRole.User);
        var expires = DateTimeOffset.UtcNow.Add(request.RememberMe ? RememberMeLifetime : SessionLifetime);

        return ToResponse(user, role, expires);
    }

    public async Task<AuthSessionResponse> GetCurrentSessionAsync(int userId, CancellationToken ct = default)
    {
        var user = await users.FindByIdAsync(userId.ToString())
            ?? throw new UnauthorizedException();

        var role = user.RoleId == 2 ? UserRole.Company : (user.RoleId == 1 ? UserRole.Admin : UserRole.User);
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
        VerificationStatus = VerificationStatus.NotSubmitted,
        ExpiresAt = expiresAt
    };
}
