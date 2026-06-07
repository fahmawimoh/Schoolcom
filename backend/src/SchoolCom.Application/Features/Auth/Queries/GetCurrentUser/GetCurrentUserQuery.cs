using MediatR;
using SchoolCom.Application.Common.Exceptions;
using SchoolCom.Application.Common.Interfaces;
using SchoolCom.Application.Features.Auth.Commands.Login;

namespace SchoolCom.Application.Features.Auth.Queries.GetCurrentUser;

public record GetCurrentUserQuery : IRequest<CurrentUserResponse>;

public record CurrentUserResponse(
    LoginUserDto User,
    int ExpiresIn);

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    private readonly ICurrentUserService _currentUser;
    private readonly IUserRepository _users;

    public GetCurrentUserQueryHandler(
        ICurrentUserService currentUser,
        IUserRepository users)
    {
        _currentUser = currentUser;
        _users = users;
    }

    public async Task<CurrentUserResponse> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not Guid userId)
            throw new AuthException("AUTH_UNAUTHORIZED", "auth.unauthorized");

        var user = await _users.GetByIdAsync(userId, cancellationToken)
            ?? throw new AuthException("AUTH_UNAUTHORIZED", "auth.unauthorized");

        if (!user.IsActive)
            throw new AuthException("AUTH_ACCOUNT_DISABLED", "auth.accountDisabled");

        var roles = user.Roles.Select(r => r.ToString()).ToList();
        var expiresIn = _currentUser.AccessTokenExpiresInSeconds ?? 15 * 60;

        return new CurrentUserResponse(
            new LoginUserDto(
                user.Id,
                user.Email,
                user.Phone,
                user.FirstName,
                user.LastName,
                roles,
                user.PreferredLanguage,
                user.TenantId),
            expiresIn);
    }
}
