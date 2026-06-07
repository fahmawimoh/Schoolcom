using FluentValidation;
using MediatR;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.Auth.Commands.Login;

namespace SchoolCom.Application.Features.Auth.Commands.VerifyOtp;

public record VerifyOtpCommand(string Identifier, string Code) : IRequest<Login.LoginResponse>;

public class VerifyOtpCommandValidator : AbstractValidator<VerifyOtpCommand>
{
    public VerifyOtpCommandValidator()
    {
        RuleFor(x => x.Identifier).NotEmpty();
        RuleFor(x => x.Code).NotEmpty().Length(6);
    }
}

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Login.LoginResponse>
{
    private readonly IOtpService _otpService;
    private readonly IUserRepository _users;
    private readonly IJwtTokenService _jwt;
    private readonly IRefreshTokenRepository _refreshTokens;
    private readonly IAuditService _audit;

    public VerifyOtpCommandHandler(
        IOtpService otpService,
        IUserRepository users,
        IJwtTokenService jwt,
        IRefreshTokenRepository refreshTokens,
        IAuditService audit)
    {
        _otpService = otpService;
        _users = users;
        _jwt = jwt;
        _refreshTokens = refreshTokens;
        _audit = audit;
    }

    public async Task<Login.LoginResponse> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
    {
        var valid = await _otpService.ValidateAsync(request.Identifier, request.Code, cancellationToken);
        if (!valid)
            throw new AuthException("AUTH_INVALID_OTP", "auth.invalidOtp");

        var user = await _users.GetByPhoneAsync(request.Identifier, cancellationToken)
            ?? await _users.GetByEmailAsync(request.Identifier, cancellationToken)
            ?? throw new AuthException("AUTH_USER_NOT_FOUND", "auth.userNotFound");

        var roles = user.Roles.Select(r => r.ToString()).ToList();
        var (accessToken, expiresIn) = _jwt.GenerateAccessToken(
            user.Id, user.TenantId, user.Email, roles, user.PreferredLanguage);

        var refreshToken = _jwt.GenerateRefreshToken();
        await _refreshTokens.CreateAsync(user.Id, _jwt.HashToken(refreshToken), cancellationToken);
        await _audit.LogAsync("OtpLogin", "User", user.Id, ct: cancellationToken);

        return new Login.LoginResponse(
            accessToken,
            expiresIn,
            new Login.LoginUserDto(
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
