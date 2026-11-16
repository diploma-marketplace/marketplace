using FutureShopBackend.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace FutureShopBackend.Infrastructure.Auth;

/// <summary>
/// Хешування паролів через bcrypt (work factor 12, з SHA-384 передхешуванням, щоб не обрізати паролі довші за 72 байти).
/// Старі PBKDF2-хеші Identity ще проходять перевірку і позначаються для перехешування.
/// </summary>
public sealed class BcryptPasswordHasher : IPasswordHasher<ApplicationUser>
{
    private const int WorkFactor = 12;
    private readonly PasswordHasher<ApplicationUser> _legacy = new();

    public string HashPassword(ApplicationUser user, string password)
        => BCrypt.Net.BCrypt.EnhancedHashPassword(password, WorkFactor);

    public PasswordVerificationResult VerifyHashedPassword(ApplicationUser user, string hashedPassword, string providedPassword)
    {
        if (!hashedPassword.StartsWith("$2", StringComparison.Ordinal))
        {
            var legacy = _legacy.VerifyHashedPassword(user, hashedPassword, providedPassword);
            return legacy == PasswordVerificationResult.Failed
                ? PasswordVerificationResult.Failed
                : PasswordVerificationResult.SuccessRehashNeeded;
        }

        bool ok;
        try { ok = BCrypt.Net.BCrypt.EnhancedVerify(providedPassword, hashedPassword); }
        catch (BCrypt.Net.SaltParseException) { return PasswordVerificationResult.Failed; }

        if (!ok) return PasswordVerificationResult.Failed;
        return BCrypt.Net.BCrypt.PasswordNeedsRehash(hashedPassword, WorkFactor)
            ? PasswordVerificationResult.SuccessRehashNeeded
            : PasswordVerificationResult.Success;
    }
}
