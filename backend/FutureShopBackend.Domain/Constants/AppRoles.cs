namespace FutureShopBackend.Domain.Constants;

/// <summary>
/// Defines application role names and role combinations for RBAC authorization.
/// </summary>
public static class AppRoles
{
    public const string Admin = "admin";
    public const string Company = "company";
    public const string User = "user";
    public const string Guest = "guest";

    public const string AdminOrCompanyOrUser = "admin,company,user,Admin,Company,User";
    public const string AdminOrCompany = "admin,company,Admin,Company";
    public const string AdminOnly = "admin,Admin";
}
