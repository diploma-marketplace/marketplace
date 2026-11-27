using FluentAssertions;
using FutureShopBackend.Api.Attributes;
using FutureShopBackend.Domain.Constants;
using FutureShopBackend.Domain.Enums;

namespace FutureShopBackend.Tests.Security;

public class RoleAuthorizationTests
{
    [Fact]
    public void AppRoles_ContainsAllRequiredRoles()
    {
        AppRoles.Admin.Should().Be("admin");
        AppRoles.Company.Should().Be("company");
        AppRoles.User.Should().Be("user");
        AppRoles.Guest.Should().Be("guest");
    }

    [Fact]
    public void UserRole_EnumDefinesAllRequiredRoles()
    {
        Enum.GetNames<UserRole>().Should().Contain(new[] { "Admin", "Company", "User", "Guest" });
    }

    [Fact]
    public void AuthorizeRolesAttribute_FormatsRolesWithBothCasing()
    {
        // Act
        var attribute = new AuthorizeRolesAttribute(UserRole.Admin, UserRole.Company);

        // Assert
        attribute.Roles.Should().NotBeNull();
        var roles = attribute.Roles!.Split(',');
        roles.Should().Contain("Admin");
        roles.Should().Contain("admin");
        roles.Should().Contain("Company");
        roles.Should().Contain("company");
    }

    [Fact]
    public void AuthorizeRolesAttribute_WithStrings_FormatsProperly()
    {
        // Act
        var attribute = new AuthorizeRolesAttribute("admin", "guest");

        // Assert
        var roles = attribute.Roles!.Split(',');
        roles.Should().Contain("admin");
        roles.Should().Contain("guest");
    }
}
