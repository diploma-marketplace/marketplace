using System.Security.Claims;
using FluentAssertions;
using FutureShopBackend.Application.Exceptions;
using FutureShopBackend.Application.Interfaces;
using FutureShopBackend.Domain.Entities;
using FutureShopBackend.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Moq;

namespace FutureShopBackend.Tests.Security;

public class ResourceOwnershipTests
{
    [Fact]
    public void EnsureOwner_WhenUnauthenticated_ThrowsUnauthorizedException()
    {
        // Arrange
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(h => h.HttpContext).Returns((HttpContext?)null);
        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);

        // Act & Assert
        var act = () => currentUserService.EnsureOwner(10);
        act.Should().Throw<UnauthorizedException>();
    }

    [Fact]
    public void EnsureOwner_WhenUserIsOwner_Succeeds()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Role, "User")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(h => h.HttpContext).Returns(context);

        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);

        // Act & Assert
        var act = () => currentUserService.EnsureOwner(42);
        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureOwner_WhenUserIsNotOwnerAndNotAdmin_ThrowsForbiddenException()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "42"),
            new Claim(ClaimTypes.Role, "User")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(h => h.HttpContext).Returns(context);

        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);

        // Act & Assert
        var act = () => currentUserService.EnsureOwner(999);
        var ex = act.Should().Throw<ForbiddenException>().Which;
        ex.Code.Should().Be("RESOURCE_OWNER_REQUIRED");
    }

    [Fact]
    public void EnsureOwner_WhenUserIsAdmin_SucceedsForAnyOwner()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Admin")
        };
        context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        httpContextAccessorMock.Setup(h => h.HttpContext).Returns(context);

        var currentUserService = new CurrentUserService(httpContextAccessorMock.Object);

        // Act & Assert
        var act = () => currentUserService.EnsureOwner(999);
        act.Should().NotThrow();
    }

    [Fact]
    public async Task ProductOwnerAuthorizationHandler_WhenOwner_SucceedsRequirement()
    {
        // Arrange
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(50);
        currentUserMock.Setup(c => c.IsAdmin).Returns(false);

        var handler = new ProductOwnerAuthorizationHandler(currentUserMock.Object);
        var product = new Product { Id = 1, SellerId = 50 };
        var requirement = new ResourceOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), product);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task ProductOwnerAuthorizationHandler_WhenNotOwner_FailsRequirement()
    {
        // Arrange
        var currentUserMock = new Mock<ICurrentUserService>();
        currentUserMock.Setup(c => c.IsAuthenticated).Returns(true);
        currentUserMock.Setup(c => c.UserId).Returns(10);
        currentUserMock.Setup(c => c.IsAdmin).Returns(false);

        var handler = new ProductOwnerAuthorizationHandler(currentUserMock.Object);
        var product = new Product { Id = 1, SellerId = 50 };
        var requirement = new ResourceOwnerRequirement();
        var context = new AuthorizationHandlerContext(new[] { requirement }, new ClaimsPrincipal(), product);

        // Act
        await handler.HandleAsync(context);

        // Assert
        context.HasSucceeded.Should().BeFalse();
    }
}
