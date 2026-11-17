using System.Security.Claims;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FutureShopBackend.Infrastructure.Auth;

/// <summary>
/// Resolves the current authenticated user's claims and identity via <see cref="IHttpContextAccessor"/>.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    /// <inheritdoc />
    public int? UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(value, out var id) ? id : null;
        }
    }

    /// <inheritdoc />
    public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

    /// <inheritdoc />
    public bool IsAdmin => string.Equals(Role, "Admin", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;
}
