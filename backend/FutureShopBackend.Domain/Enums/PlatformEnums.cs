namespace FutureShopBackend.Domain.Enums;

/// <summary>
/// Entity type targeted by a user complaint or moderation report.
/// </summary>
public enum ReportTargetType
{
    Product,
    Review,
    User
}

/// <summary>
/// Moderation status of a submitted report.
/// </summary>
public enum ReportStatus
{
    Pending,
    InReview,
    Resolved,
    Dismissed
}

/// <summary>
/// Supported UI localization languages.
/// </summary>
public enum InterfaceLanguage
{
    Uk,
    En
}

/// <summary>
/// Supported UI color themes.
/// </summary>
public enum ThemeMode
{
    Light,
    Dark,
    System
}
