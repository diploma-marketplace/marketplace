using FutureShopBackend.Domain.Enums;
using Microsoft.AspNetCore.Authorization;

namespace FutureShopBackend.Api.Attributes;

/// <summary>
/// Custom authorization attribute that verifies user membership in specified roles (supports case-insensitive matching).
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public sealed class AuthorizeRolesAttribute : AuthorizeAttribute
{
    public AuthorizeRolesAttribute(params UserRole[] roles)
    {
        var roleNames = roles
            .SelectMany(r => new[] { r.ToString(), r.ToString().ToLowerInvariant() })
            .Distinct();
        Roles = string.Join(",", roleNames);
    }

    public AuthorizeRolesAttribute(params string[] roles)
    {
        var roleNames = roles
            .SelectMany(r => new[] { r, r.ToLowerInvariant() })
            .Distinct();
        Roles = string.Join(",", roleNames);
    }
}
