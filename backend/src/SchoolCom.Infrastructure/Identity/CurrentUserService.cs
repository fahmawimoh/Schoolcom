using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Infrastructure.Identity;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId
    {
        get
        {
            var sub = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public Guid? TenantId
    {
        get
        {
            var tenant = _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");
            return Guid.TryParse(tenant, out var id) ? id : null;
        }
    }

    public IReadOnlyList<string> Roles =>
        _httpContextAccessor.HttpContext?.User?
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? [];

    public string? PreferredLanguage =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue("preferred_language");

    public string? Jti =>
        _httpContextAccessor.HttpContext?.User?.FindFirstValue(JwtRegisteredClaimNames.Jti);

    public int? AccessTokenExpiresInSeconds
    {
        get
        {
            var expClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("exp")?.Value;
            if (!long.TryParse(expClaim, out var expUnix)) return null;

            var remaining = (int)(expUnix - DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            return Math.Max(remaining, 0);
        }
    }
}
