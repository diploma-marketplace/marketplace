using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography;

namespace FutureShopBackend.Infrastructure.Auth;

/// <summary>
/// Зберігає автентифікаційну сесію в Redis. У cookie лежить лише випадковий ключ сесії,
/// самі дані (claims) браузер ніколи не бачить.
/// </summary>
public sealed class RedisTicketStore(IDistributedCache cache) : ITicketStore
{
    private const string KeyPrefix = "session:";

    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        var key = KeyPrefix + Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        await RenewAsync(key, ticket);
        return key;
    }

    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        var options = new DistributedCacheEntryOptions();
        if (ticket.Properties.ExpiresUtc is { } expires)
            options.AbsoluteExpiration = expires;

        await cache.SetAsync(key, TicketSerializer.Default.Serialize(ticket), options);
    }

    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        var bytes = await cache.GetAsync(key);
        return bytes is null ? null : TicketSerializer.Default.Deserialize(bytes);
    }

    public Task RemoveAsync(string key) => cache.RemoveAsync(key);
}
