namespace SchoolCom.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    IReadOnlyList<string> Roles { get; }
    string? PreferredLanguage { get; }
    string? Jti { get; }
    int? AccessTokenExpiresInSeconds { get; }
}

public interface IJwtTokenBlacklistService
{
    Task RevokeAsync(string jti, TimeSpan ttl, CancellationToken ct = default);
    Task<bool> IsRevokedAsync(string jti, CancellationToken ct = default);
}

public interface IJwtTokenService
{
    (string AccessToken, int ExpiresInSeconds) GenerateAccessToken(
        Guid userId,
        Guid tenantId,
        string email,
        IEnumerable<string> roles,
        string preferredLanguage);

    string GenerateRefreshToken();
    string HashToken(string token);
}

public interface IOtpService
{
    Task<string> GenerateAndStoreAsync(string identifier, Domain.Enums.OtpChannel channel, CancellationToken ct);
    Task<bool> ValidateAsync(string identifier, string code, CancellationToken ct);
}

public interface IWhatsAppService
{
    Task<(bool Connected, string? PhoneNumberId)> GetConfigAsync(Guid tenantId, CancellationToken ct);
    Task<string> SendMessageAsync(Guid tenantId, Guid adminUserId, string phone, string message, CancellationToken ct);
    Task<IReadOnlyList<WhatsAppConversationDto>> GetConversationsAsync(Guid tenantId, CancellationToken ct);
}

public record WhatsAppConversationDto(string Id, string Phone, string LastMessage, DateTime Timestamp);

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface IAuditService
{
    Task LogAsync(string action, string entityType, Guid? entityId, object? metadata = null, CancellationToken ct = default);
}
