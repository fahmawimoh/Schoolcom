using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Auth.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<LoginResponse>;

public record LoginResponse(
    string AccessToken,
    int ExpiresIn,
    LoginUserDto User);

public record LoginUserDto(
    Guid Id,
    string Email,
    string? Phone,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    string PreferredLanguage,
    Guid TenantId);

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResponse>
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IAuditService _audit;

    public LoginCommandHandler(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwt,
        IRefreshTokenRepository refreshTokens,
        IAuditService audit)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _jwt = jwt;
        _refreshTokens = refreshTokens;
        _audit = audit;
    }

    public async Task<LoginResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _users.GetByEmailAsync(request.Email, cancellationToken)
            ?? throw new AuthException("AUTH_INVALID_CREDENTIALS", "auth.invalidCredentials");

        if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            throw new AuthException("AUTH_INVALID_CREDENTIALS", "auth.invalidCredentials");

        if (!user.IsActive)
            throw new AuthException("AUTH_ACCOUNT_DISABLED", "auth.accountDisabled");

        var roles = user.Roles.Select(r => r.ToString()).ToList();
        var (accessToken, expiresIn) = _jwt.GenerateAccessToken(
            user.Id, user.TenantId, user.Email, roles, user.PreferredLanguage);

        var refreshToken = _jwt.GenerateRefreshToken();
        await _refreshTokens.CreateAsync(user.Id, _jwt.HashToken(refreshToken), cancellationToken);
        await _audit.LogAsync("Login", "User", user.Id, ct: cancellationToken);

        return new LoginResponse(
            accessToken,
            expiresIn,
            new LoginUserDto(
                user.Id,
                user.Email,
                user.Phone,
                user.FirstName,
                user.LastName,
                roles,
                user.PreferredLanguage,
                user.TenantId));
    }
}

public interface IUserRepository
{
    Task<UserAuthModel?> GetByEmailAsync(string email, CancellationToken ct);
    Task<UserAuthModel?> GetByPhoneAsync(string phone, CancellationToken ct);
    Task<UserAuthModel?> GetByIdAsync(Guid id, CancellationToken ct);
}

public interface IRefreshTokenRepository
{
    Task CreateAsync(Guid userId, string tokenHash, CancellationToken ct);
    Task RevokeAllForUserAsync(Guid userId, CancellationToken ct);
}

public record UserAuthModel(
    Guid Id,
    Guid TenantId,
    string Email,
    string? Phone,
    string PasswordHash,
    string FirstName,
    string LastName,
    string PreferredLanguage,
    bool IsActive,
    IReadOnlyList<Domain.Enums.UserRole> Roles);
