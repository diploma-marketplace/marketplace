using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace FutureShopBackend.Infrastructure.Auth;

/// <summary>
/// Authorization requirement that requires the acting user to be the owner of the resource or an administrator.
/// </summary>
public sealed class ResourceOwnerRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// Authorizes access to a <see cref="Product"/> only if the current user is the listing's seller or an administrator.
/// </summary>
public sealed class ProductOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, Product>
{
    private readonly ICurrentUserService _currentUser;

    public ProductOwnerAuthorizationHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        Product resource)
    {
        if (_currentUser.IsAuthenticated && (_currentUser.IsAdmin || _currentUser.UserId == resource.SellerId))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}

/// <summary>
/// Authorizes access to a resource by owner ID only if the current user is the owner or an administrator.
/// </summary>
public sealed class DirectOwnerAuthorizationHandler : AuthorizationHandler<ResourceOwnerRequirement, int>
{
    private readonly ICurrentUserService _currentUser;

    public DirectOwnerAuthorizationHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerRequirement requirement,
        int resourceOwnerId)
    {
        if (_currentUser.IsAuthenticated && (_currentUser.IsAdmin || _currentUser.UserId == resourceOwnerId))
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }

        return Task.CompletedTask;
    }
}
