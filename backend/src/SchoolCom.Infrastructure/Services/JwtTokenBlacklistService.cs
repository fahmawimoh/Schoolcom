using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SchoolCom.Application.Common.Interfaces;
using StackExchange.Redis;

namespace SchoolCom.Infrastructure.Services;

public class JwtTokenBlacklistService(
    IDistributedCache cache,
    ILogger<JwtTokenBlacklistService> logger) : IJwtTokenBlacklistService
{
    private const string KeyPrefix = "jwt:revoked:";

    public async Task RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti) || ttl <= TimeSpan.Zero)
            return;

        try
        {
            await cache.SetStringAsync(
                Key(jti),
                "1",
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = ttl
                },
                ct);
        }
        catch (Exception ex) when (ex is RedisConnectionException or RedisTimeoutException)
        {
            logger.LogWarning(ex, "Redis unavailable; token revocation was not persisted for jti {Jti}", jti);
        }
    }

    public async Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jti))
            return false;

        try
        {
            var value = await cache.GetStringAsync(Key(jti), ct);
            return value is not null;
        }
        catch (Exception ex) when (ex is RedisConnectionException or RedisTimeoutException)
        {
            logger.LogWarning(ex, "Redis unavailable; skipping JWT blacklist check for jti {Jti}", jti);
            return false;
        }
    }

    private static string Key(string jti) => $"{KeyPrefix}{jti}";
}
