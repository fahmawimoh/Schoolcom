using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Domain.Entities;
using SchoolCom.Domain.Enums;
using SchoolCom.Infrastructure.Persistence;

namespace SchoolCom.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly IDistributedCache _cache;
    private readonly ApplicationDbContext _db;

    public OtpService(IDistributedCache cache, ApplicationDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    public async Task<string> GenerateAndStoreAsync(
        string identifier,
        OtpChannel channel,
        CancellationToken ct)
    {
        var code = Random.Shared.Next(100000, 999999).ToString();
        var hash = Hash(code);

        await _cache.SetStringAsync(
            CacheKey(identifier),
            hash,
            new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) },
            ct);

        _db.OtpCodes.Add(new OtpCode
        {
            Identifier = identifier,
            CodeHash = hash,
            Channel = channel,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5)
        });
        await _db.SaveChangesAsync(ct);

        // In production: send via SMS/email provider
        Console.WriteLine($"[DEV OTP] {identifier}: {code}");

        return code;
    }

    public async Task<bool> ValidateAsync(string identifier, string code, CancellationToken ct)
    {
        var cached = await _cache.GetStringAsync(CacheKey(identifier), ct);
        if (cached is null) return false;

        var valid = cached == Hash(code);
        if (valid)
            await _cache.RemoveAsync(CacheKey(identifier), ct);

        return valid;
    }

    private static string CacheKey(string identifier) => $"otp:{identifier}";
    private static string Hash(string code)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }
}

public class WhatsAppBusinessService : IWhatsAppService
{
    private readonly ApplicationDbContext _db;
    private readonly IConfiguration _configuration;

    public WhatsAppBusinessService(ApplicationDbContext db, IConfiguration configuration)
    {
        _db = db;
        _configuration = configuration;
    }

    public async Task<(bool Connected, string? PhoneNumberId)> GetConfigAsync(
        Guid tenantId,
        CancellationToken ct)
    {
        var session = await _db.WhatsAppSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsActive, ct);

        if (session is null)
        {
            var devMode = _configuration.GetValue<bool>("WhatsApp:DevMode");
            return devMode
                ? (true, _configuration["WhatsApp:DevPhoneNumberId"] ?? "dev-phone-id")
                : (false, null);
        }

        return (true, session.PhoneNumberId);
    }

    public async Task<string> SendMessageAsync(
        Guid tenantId,
        Guid adminUserId,
        string phone,
        string message,
        CancellationToken ct)
    {
        var session = await _db.WhatsAppSessions
            .FirstOrDefaultAsync(s => s.TenantId == tenantId && s.IsActive, ct);

        var messageId = Guid.NewGuid().ToString();

        // Production: call Meta WhatsApp Business Cloud API
        // POST https://graph.facebook.com/v21.0/{phone-number-id}/messages
        if (_configuration.GetValue<bool>("WhatsApp:DevMode") || session is null)
        {
            Console.WriteLine($"[DEV WhatsApp] To {phone}: {message}");
        }

        _db.WhatsAppMessages.Add(new WhatsAppMessage
        {
            TenantId = tenantId,
            SessionId = session?.Id ?? Guid.Empty,
            RecipientPhone = phone,
            Body = message,
            Direction = "Outbound",
            ExternalMessageId = messageId,
            Status = "Sent"
        });
        await _db.SaveChangesAsync(ct);

        return messageId;
    }

    public async Task<IReadOnlyList<WhatsAppConversationDto>> GetConversationsAsync(
        Guid tenantId,
        CancellationToken ct)
    {
        return await _db.WhatsAppMessages
            .AsNoTracking()
            .Where(m => m.TenantId == tenantId)
            .GroupBy(m => m.RecipientPhone)
            .Select(g => new WhatsAppConversationDto(
                g.OrderByDescending(m => m.CreatedAt).First().Id.ToString(),
                g.Key,
                g.OrderByDescending(m => m.CreatedAt).First().Body,
                g.Max(m => m.CreatedAt)))
            .OrderByDescending(c => c.Timestamp)
            .Take(20)
            .ToListAsync(ct);
    }
}

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContext;

    public AuditService(
        ApplicationDbContext db,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContext)
    {
        _db = db;
        _currentUser = currentUser;
        _httpContext = httpContext;
    }

    public async Task LogAsync(
        string action,
        string entityType,
        Guid? entityId,
        object? metadata = null,
        CancellationToken ct = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            TenantId = _currentUser.TenantId ?? Guid.Empty,
            UserId = _currentUser.UserId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            MetadataJson = metadata is null ? null : System.Text.Json.JsonSerializer.Serialize(metadata),
            IpAddress = _httpContext.HttpContext?.Connection.RemoteIpAddress?.ToString()
        });
        await _db.SaveChangesAsync(ct);
    }
}
