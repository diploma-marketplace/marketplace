namespace FutureShopBackend.Domain.Enums;

/// <summary>
/// System roles defining RBAC permissions across the marketplace.
/// </summary>
public enum UserRole
{
    /// <summary>Individual buyer, private seller, or sole proprietor (FOP).</summary>
    User,

    /// <summary>Registered legal entity operating exclusively as a seller.</summary>
    Company,

    /// <summary>Platform administrator and marketplace operator.</summary>
    Admin
}

/// <summary>
/// Lifecycle status of a user or company account.
/// </summary>
public enum AccountStatus
{
    PendingContactVerification,
    Active,
    TemporarilyBlocked,
    PermanentlyBlocked,
    Deactivated
}

/// <summary>
/// Verification state for FOP and legal-entity (company) sellers.
/// </summary>
public enum VerificationStatus
{
    NotSubmitted,
    Pending,
    Approved,
    Rejected
}

/// <summary>
/// Type of seller offering a product on the marketplace.
/// </summary>
public enum SellerType
{
    /// <summary>Official marketplace inventory sold directly by the platform.</summary>
    Marketplace,

    /// <summary>Inventory sold by third-party individuals, FOPs, or companies.</summary>
    ThirdParty
}
