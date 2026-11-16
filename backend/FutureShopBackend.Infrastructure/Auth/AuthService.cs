using FutureShopBackend.Application.DTO.Auth;
using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FutureShopBackend.Infrastructure.Auth;

/// <summary>
/// Реєстрація, вхід і вихід. Паролі хешує ASP.NET Core Identity (PBKDF2 + сіль), сесія живе в Redis.
/// </summary>
public sealed class AuthService(
    UserManager<ApplicationUser> users,
    IHttpContextAccessor accessor) : IAuthService
{
    private static readonly TimeSpan SessionLifetime = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan RememberMeLifetime = TimeSpan.FromDays(14);

    private HttpContext Http => accessor.HttpContext
        ?? throw new InvalidOperationException("No active HTTP context.");

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
            StatusType = request.Role == UserRole.Company ? SellerStatusType.Company : null,
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

        return await SignInAsync(user, request.Role, rememberMe: false);
    }

    public async Task<AuthSessionResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.NormalizedEmail == request.Email.ToUpperInvariant(), ct);

        // Однакова відповідь для "немає користувача" і "поганий пароль" — не розкриваємо, які email існують.
        if (user is null || !await users.CheckPasswordAsync(user, request.Password))
            throw new UnauthorizedException("Invalid email or password.", "INVALID_CREDENTIALS");

        if (await users.IsLockedOutAsync(user))
            throw new ForbiddenException("Account is temporarily blocked.", "ACCOUNT_BLOCKED");

        return await SignInAsync(user, ParseRole(user.Role.Name), request.RememberMe);
    }

    public async Task<MessageResponse> LogoutAsync(CancellationToken ct = default)
    {
        // Видаляє сесію з Redis і стирає cookie.
        await Http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return new MessageResponse { Message = "Logged out." };
    }

    public async Task<AuthSessionResponse> GetCurrentSessionAsync(CancellationToken ct = default)
    {
        if (!int.TryParse(Http.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id))
            throw new UnauthorizedException();

        var user = await users.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Id == id, ct)
            ?? throw new UnauthorizedException();

        var auth = await Http.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return ToResponse(user, ParseRole(user.Role.Name), auth.Properties?.ExpiresUtc ?? DateTimeOffset.UtcNow.Add(SessionLifetime));
    }

    public Task<MessageResponse> VerifyContactAsync(VerifyContactRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Contact verification is not part of this sprint.");

    public Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Password recovery is not part of this sprint.");

    public Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("Password recovery is not part of this sprint.");

    private async Task<AuthSessionResponse> SignInAsync(ApplicationUser user, UserRole role, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email!),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        var expires = DateTimeOffset.UtcNow.Add(rememberMe ? RememberMeLifetime : SessionLifetime);
        var props = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            AllowRefresh = !rememberMe,
            ExpiresUtc = expires
        };

        // Метадані сесії зберігаються разом із нею в Redis
        var userAgent = Http.Request.Headers.UserAgent.ToString();
        props.Items["ip"] = Http.Connection.RemoteIpAddress?.ToString();
        props.Items["user_agent"] = userAgent.Length > 256 ? userAgent[..256] : userAgent;
        props.Items["created_at"] = DateTimeOffset.UtcNow.ToString("O");

        await Http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity), props);
        return ToResponse(user, role, expires);
    }

    private static UserRole ParseRole(string? name)
        => Enum.TryParse<UserRole>(name, ignoreCase: true, out var r) ? r : UserRole.User;

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
