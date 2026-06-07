using MediatR;
using SchoolCom.Application.Common.Interfaces;

namespace SchoolCom.Application.Features.Messaging.Queries;

public record GetPendingModerationQuery(ModerationFilter? Filter = null)
    : IRequest<IReadOnlyList<PendingModerationDto>>;

public record GetModerationHistoryQuery(
    int Limit = 50,
    ModerationFilter? Filter = null) : IRequest<IReadOnlyList<ModerationHistoryDto>>;

public record GetNotificationsQuery : IRequest<IReadOnlyList<NotificationDto>>;

public class GetPendingModerationQueryHandler(
    IMessagingRepository repository,
    ICurrentUserService currentUser) : IRequestHandler<GetPendingModerationQuery, IReadOnlyList<PendingModerationDto>>
{
    public Task<IReadOnlyList<PendingModerationDto>> Handle(
        GetPendingModerationQuery request,
        CancellationToken cancellationToken)
    {
        if (!MessagingAuthorization.IsAdmin(currentUser.Roles))
            throw new UnauthorizedAccessException("Admin access required.");

        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return repository.GetPendingApprovalsAsync(
            tenantId, request.Filter ?? new ModerationFilter(), cancellationToken);
    }
}

public class GetModerationHistoryQueryHandler(
    IMessagingRepository repository,
    ICurrentUserService currentUser) : IRequestHandler<GetModerationHistoryQuery, IReadOnlyList<ModerationHistoryDto>>
{
    public Task<IReadOnlyList<ModerationHistoryDto>> Handle(
        GetModerationHistoryQuery request,
        CancellationToken cancellationToken)
    {
        if (!MessagingAuthorization.IsAdmin(currentUser.Roles))
            throw new UnauthorizedAccessException("Admin access required.");

        var tenantId = currentUser.TenantId
            ?? throw new UnauthorizedAccessException("Tenant not found");

        return repository.GetModerationHistoryAsync(
            tenantId, request.Filter ?? new ModerationFilter(), request.Limit, cancellationToken);
    }
}

public class GetNotificationsQueryHandler(
    IMessagingRepository repository,
    IMessagingAccessService access) : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    public Task<IReadOnlyList<NotificationDto>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var tenantId = access.RequireTenantId();
        var userId = access.RequireUserId();
        return repository.GetNotificationsAsync(tenantId, userId, cancellationToken);
    }
}
