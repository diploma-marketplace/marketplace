namespace FutureShopBackend.Application.Interfaces;
/// <summary>
/// Provides access to the currently authenticated user's contextual identity and roles in the request.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Identifier of the current authenticated user (resolved from <see cref="System.Security.Claims.ClaimTypes.NameIdentifier"/>).
    /// Returns <c>null</c> if the request is anonymous.
    /// </summary>
    int? UserId { get; }

    /// <summary>
    /// Role of the current authenticated user (resolved from <see cref="System.Security.Claims.ClaimTypes.Role"/>).
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Indicates whether the current authenticated user has administrative privileges.
    /// </summary>
    bool IsAdmin { get; }

    /// <summary>
    /// Indicates whether the current authenticated user has company seller privileges.
    /// </summary>
    bool IsCompany { get; }

    /// <summary>
    /// Indicates whether the current authenticated user has regular user privileges.
    /// </summary>
    bool IsUser { get; }

    /// <summary>
    /// Indicates whether the current authenticated user is a guest.
    /// </summary>
    bool IsGuest { get; }

    /// <summary>
    /// Indicates whether the current request is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Ensures that the authenticated account owns the resource, unless it is an administrator.
    /// The owner id must come from the database, never from the request body or route.
    /// </summary>
    void EnsureOwner(int ownerId);
}
