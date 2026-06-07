using Microsoft.EntityFrameworkCore;
using SchoolCom.Application.Features.Auth.Commands.Login;
using SchoolCom.Domain.Enums;
using SchoolCom.Infrastructure.Persistence;

namespace SchoolCom.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _db;

    public UserRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<UserAuthModel?> GetByEmailAsync(string email, CancellationToken ct)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.RoleAssignments)
            .ThenInclude(ra => ra.Role)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower(), ct);

        return Map(user);
    }

    public async Task<UserAuthModel?> GetByPhoneAsync(string phone, CancellationToken ct)
    {
        var normalized = NormalizePhone(phone);
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.RoleAssignments)
            .ThenInclude(ra => ra.Role)
            .FirstOrDefaultAsync(u => u.Phone != null && u.Phone.Replace(" ", "") == normalized, ct);

        return Map(user);
    }

    public async Task<UserAuthModel?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var user = await _db.Users
            .AsNoTracking()
            .Include(u => u.RoleAssignments)
            .ThenInclude(ra => ra.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

        return Map(user);
    }

    private static UserAuthModel? Map(Domain.Entities.User? user)
    {
        if (user is null) return null;

        return new UserAuthModel(
            user.Id,
            user.TenantId,
            user.Email,
            user.Phone,
            user.PasswordHash,
            user.FirstName,
            user.LastName,
            user.PreferredLanguage,
            user.IsActive,
            user.RoleAssignments.Select(ra => ra.Role.Name).ToList());
    }

    private static string NormalizePhone(string phone) =>
        phone.Replace(" ", "").Replace("-", "");
}

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _db;

    public RefreshTokenRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(Guid userId, string tokenHash, CancellationToken ct)
    {
        _db.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        });
        await _db.SaveChangesAsync(ct);
    }

    public async Task RevokeAllForUserAsync(Guid userId, CancellationToken ct)
    {
        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(ct);

        foreach (var token in tokens)
            token.RevokedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);
    }
}
