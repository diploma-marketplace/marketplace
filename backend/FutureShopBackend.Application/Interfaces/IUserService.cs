using FutureShopBackend.Application.DTO.Common;
using FutureShopBackend.Application.DTO.Users;

namespace FutureShopBackend.Application.Interfaces;

/// <summary>
/// Application service contract for managing the authenticated user's private profile, credentials, verification, and analytics.
/// </summary>
public interface IUserService
{
    Task<UserProfileResponse> GetMyProfileAsync(CancellationToken cancellationToken = default);
    Task<UserProfileResponse> UpdateMyProfileAsync(UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<MessageResponse> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<SellerVerificationResponse> SubmitVerificationAsync(SubmitSellerVerificationRequest request, CancellationToken cancellationToken = default);
    Task<SellerAnalyticsResponse> GetMySellerAnalyticsAsync(CancellationToken cancellationToken = default);
    Task DeactivateMyAccountAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Application service contract for public seller and company profile queries.
/// </summary>
public interface ISellerService
{
    Task<PublicSellerProfileResponse> GetSellerProfileAsync(Guid sellerId, CancellationToken cancellationToken = default);
}
