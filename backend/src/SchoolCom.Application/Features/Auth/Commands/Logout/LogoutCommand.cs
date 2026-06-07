using MediatR;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.Auth.Commands.Login;

namespace SchoolCom.Application.Features.Auth.Commands.Logout;

public record LogoutCommand : IRequest;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IJwtTokenBlacklistService _tokenBlacklist;
    private readonly IAuditService _audit;

    public LogoutCommandHandler(
        ICurrentUserService currentUser,
        IRefreshTokenRepository refreshTokens,
        IJwtTokenBlacklistService tokenBlacklist,
        IAuditService audit)
    {
        _currentUser = currentUser;
        _refreshTokens = refreshTokens;
        _tokenBlacklist = tokenBlacklist;
        _audit = audit;
    }

    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
            throw new AuthException("AUTH_UNAUTHORIZED", "auth.unauthorized");

        if (!string.IsNullOrWhiteSpace(_currentUser.Jti))
        {
            var ttlSeconds = _currentUser.AccessTokenExpiresInSeconds ?? 15 * 60;
            await _tokenBlacklist.RevokeAsync(
                _currentUser.Jti,
                TimeSpan.FromSeconds(Math.Max(ttlSeconds, 1)),
                cancellationToken);
        }

        await _refreshTokens.RevokeAllForUserAsync(userId, cancellationToken);
        await _audit.LogAsync("Logout", "User", userId, ct: cancellationToken);
    }
}
