using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

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

    public void EnsureOwner(int ownerId)
    {
        if (!IsAuthenticated || UserId is null)
        {
            throw new UnauthorizedException();
        }

        if (!IsAdmin && UserId.Value != ownerId)
        {
            throw new ForbiddenException(
                "You can access only resources owned by your account.",
                "RESOURCE_OWNER_REQUIRED");
        }
    }
}
