using FutureShopBackend.Application.DTO.Auth;
using FutureShopBackend.Application.DTO.Common;

namespace FutureShopBackend.Application.Interfaces;

/// <summary>
/// Application service contract for user/company registration, session cookie authentication, and password recovery.
/// </summary>
public interface IAuthService
{
    Task<AuthSessionResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<AuthSessionResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<MessageResponse> LogoutAsync(CancellationToken cancellationToken = default);
    Task<AuthSessionResponse> GetCurrentSessionAsync(CancellationToken cancellationToken = default);
    Task<MessageResponse> VerifyContactAsync(VerifyContactRequest request, CancellationToken cancellationToken = default);
    Task<MessageResponse> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<MessageResponse> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
